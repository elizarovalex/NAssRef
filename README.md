NAssRef
=======

.NET assembly reference conflict checker

## Example

### Bin directory:

    .\Demo.Test.Main.exe
    .\Demo.Test.First.dll
    .\Demo.Test.Second.dll
    .\Demo.Test.Util.dll (v 2.0)

### Refernce map

    1 Demo.Test.Main.exe -> Demo.Test.First.dll
    2 Demo.Test.Main.exe -> Demo.Test.Second.dll
    
    3 Demo.Test.First.dll -> Demo.Test.Util.dll (v 1.0)
    4 Demo.Test.Second.dll -> Demo.Test.Util.dll (v 2.0)

### Result
3 and 4 is conflict references

## NAssRef main capabilities
- UI mode
- Console mode (for continuous integration)
- Regex filter conflicts

## License
NAssRef is licensed under [MIT License](http://en.wikipedia.org/wiki/MIT_License)
