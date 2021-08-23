Merges records, sorts, and outputs a file

    dotnet run -- --files ./data/input1 ./data/input2 ./data/input3 --sort FavoriteColor:asc LastName:asc --output output
    dotnet run -- --files ./data/input1 ./data/input2 ./data/input3 --sort DateOfBirth:asc --output output
    dotnet run -- --files ./data/input1 ./data/input2 ./data/input3 --sort LastName:desc --output output