using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecordMergerAPI.Models;
using RecordMerger;

namespace RecordMergerAPI.Controllers
{
    [Route("records")]
    [ApiController]
    public class RecordItemsController : ControllerBase
    {
        private readonly RecordContext _context;
        private StartupCLI _startup;

        public RecordItemsController(RecordContext context)
        {
            _context = context;
            _startup = new StartupCLI();
        }

        // GET: records/color
        [HttpGet("{sort}")]
        public async Task<ActionResult<CSV>> GetRecordItems(string sort)
        {   
            var recordItems = await _context.RecordItems.ToListAsync();
            
            try
            {
                var csv = GetCSV(sort, recordItems);
                return csv;
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private CSV GetCSV(string sort, List<RecordItem> recordItems)
        {
            var rows = GetRowsFromRecordItems(recordItems);
            var positionsByColumnName = new Dictionary<string, int>()
            {
                {"LastName", 0},
                {"FirstName", 1},
                {"Email", 2},
                {"FavoriteColor", 3},
                {"DateOfBirth", 4}
            };

            var sorts = new string[] { };
            sort = sort.ToLower();

            if (sort == "color")
            {
                sorts = new string[] { "FavoriteColor:asc" };
            }
            else if (sort == "birthdate")
            {
                sorts = new string[] { "DateOfBirth:asc" };
            }
            else if (sort == "name")
            {
                sorts = new string[] { "LastName:asc" };
            }

            var sortsArr = _startup.GetSorts(sorts, positionsByColumnName);
            rows = _startup.SortRows(rows, sortsArr);

            var csvBody = _startup.GetCSVBody(rows);
            var csv = new CSV();
            csv.Body = csvBody;

            return csv;
        }

        private List<string[]> GetRowsFromRecordItems(List<RecordItem> recordItems)
        {
            List<string[]> rows = new List<string[]>();

            foreach (var recordItem in recordItems)
            {
                var row = _startup.GetRow(recordItem.Line, recordItem.Delimiter);
                rows.Add(row);
            }

            return rows;
        }

        // GET: records/api/5
        [HttpGet("api/{id}")]
        public async Task<ActionResult<RecordItem>> GetRecordItem(long id)
        {
            var recordItem = await _context.RecordItems.FindAsync(id);

            if (recordItem == null)
            {
                return NotFound();
            }

            return recordItem;
        }

        // POST: records
        [HttpPost]
        public async Task<ActionResult<RecordItem>> PostRecordItem(RecordItemDTO recordItemDTO)
        {
            var delimiter = _startup.GetDelimiter(recordItemDTO.Line);
            if (delimiter == '\0')
            {
                return BadRequest("Invalid delimiter.");
            }
            
            var recordItem = new RecordItem();
            recordItem.Delimiter = delimiter;
            recordItem.Line = recordItemDTO.Line;

            _context.RecordItems.Add(recordItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecordItem), new { id = recordItem.Id }, recordItem);
        }
    }
}
