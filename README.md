# Hake Command
custom command line

## User Input
The basic syntax of the supported user input should be like follows:
```
    COMMAND [ARGUMENTS]
```
Arguments can be the combination of the following items:

1. Switch: `-Name`
2. Option: `-Name value`
3. Argument: `value`

Argument values can only be parsed as `string` or `string[]`, the symbol `;` is used to sperate each of values within the same array. Type of values will be automatically converted.

Pipe symbol `|` should be appended if another command would be joined. The result of the former command will be passed as input object to the next command.

Some samples:
```
command 123 -NoWrite -Input abc;def;ghi
ls | at -index 0 | type
```

## Framework

[`Pipeline`](https://github.com/lzl1918/Hake.Extension.Pipeline) is implemented to process user inputs.


### CommandSet
`CommandLoader` will search for classes that derived from abstract class `CommandSet` to retrive the commands.

A valid command should be a public method with `Command` attribute.

### Object Wrapping
The command can return any type of object. Sometimes the output information can be displayed with more details. The value can be wrapped with a new class and its method `OnWrite` (if exists) will be invoked to retrive the displaying object. Since the value may need to be passed as the input of the next command, `OnGetValue` method will be called.

Most of the time, there is no need to manually unwrap input objects. But in the listed situations the unwrap operation may need to be considered:

1. The input object is a list of elements, and the element inside may be wrapped
2. The input object is wrapped more than once and the framework will unwrap it only one time.