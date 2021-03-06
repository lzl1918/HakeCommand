﻿using Hake.Extension.StateMachine;
using HakeCommand.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Input.Internal
{
    internal static class TransformationBuilderExtensions
    {
        public static ITransformationBuilder<int, char> OnPipeSymbol(this ITransformationBuilder<int, char> builder, int newState, TriggeringAction<int, char> triggeringAction)
            => builder.OnValue(CharCategoryHelper.PIPE_CHAR, newState, triggeringAction);
        public static ITransformationBuilder<int, char> OnPipeSymbolKeep(this ITransformationBuilder<int, char> builder, TriggeringAction<int, char> triggeringAction)
            => builder.OnValueKeep(CharCategoryHelper.PIPE_CHAR, triggeringAction);

        public static ITransformationBuilder<int, char> OnEscapeSymbol(this ITransformationBuilder<int, char> builder, int newState, TriggeringAction<int, char> triggeringAction)
            => builder.OnValue(CharCategoryHelper.ESCAPE_CHAR, newState, triggeringAction);
        public static ITransformationBuilder<int, char> OnEscapeSymbol(this ITransformationBuilder<int, char> builder, int newState)
            => builder.OnValue(CharCategoryHelper.ESCAPE_CHAR, newState, null);
    }

    internal static class Conditions
    {
        public static bool IsWhitespace(int state, char input) => input.IsWhitespace();
        public static bool IsValidFirstCharacter(int state, char input) => input.IsValidFirstCharacter();
        public static bool IsValidCharacter(int state, char input) => input.IsValidCharacter();
        public static bool IsValidInput(int state, char input) => input.IsValidInput();
        public static bool IsOptionNameStart(int state, char input) => input == '-';
    }

    internal sealed class InputParser
    {
        private IStateMachine<int, char> parseMachine;
        private StringBuilder valueBuilder;
        private string name;
        private string rawInput;
        private string errorMessage;
        private IList<IInput> inputs;
        private Dictionary<string, object> options;
        private List<object> arguments;
        private List<string> valueArray;
        private string key;

        public InputParser()
        {
            valueBuilder = new StringBuilder(64);
            inputs = new List<IInput>();
            options = new Dictionary<string, object>();
            arguments = new List<object>();
            valueArray = new List<string>();

            parseMachine = ConfigureParseMachine();
        }

        public IInputCollection Parse(string input)
        {
            rawInput = input;
            input = input.Trim().ToLower();
            errorMessage = null;
            parseMachine.Invoke(0, input);
            if (errorMessage != null)
                return new InputCollection(rawInput, errorMessage);
            List<IInput> parsedInputs = new List<IInput>(inputs);
            inputs.Clear();
            return new InputCollection(rawInput, parsedInputs);
        }
        private IStateMachine<int, char> ConfigureParseMachine()
        {
            IStateMachine<int, char> stateMachine = new StateMachine<int, char>();

            stateMachine.Configure(0)
                .OnConditionKeep(Conditions.IsWhitespace)
                .OnCondition(Conditions.IsValidFirstCharacter, 1, AddInputToValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(1)
                .OnCondition(Conditions.IsWhitespace, 2, SaveCommandName)
                .OnPipeSymbol(0, SaveCommandNameAndSaveCommand)
                .OnConditionKeep(Conditions.IsValidCharacter, AddInputToValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(2)
                .OnConditionKeep(Conditions.IsWhitespace)
                .OnPipeSymbol(0, SaveCommand)
                .OnCondition(Conditions.IsOptionNameStart, 3)
                .OnEscapeSymbol(5)
                .OnValue('"', 6)
                .OnCondition(Conditions.IsValidFirstCharacter, 4, AddInputToValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(3)
                .OnCondition(Conditions.IsWhitespace, 2)
                .OnPipeSymbol(0, SaveCommand)
                .OnCondition(Conditions.IsValidFirstCharacter, 8, AddInputToValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(4)
                .OnValue(';', 9, AddValueBuilderToValueArray)
                .OnPipeSymbol(0, AddArgumentAndSaveCommand)
                .OnEscapeSymbol(5)
                .OnCondition(Conditions.IsWhitespace, 2, AddValueBuilderToArguments)
                .OnConditionKeep(Conditions.IsValidCharacter, AddInputToValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(5)
                .OnAlways(4, AddInputToValueBuilder);

            stateMachine.Configure(6)
                .OnValue('"', 10)
                .OnEscapeSymbol(7)
                .OnAlways(6, AddInputToValueBuilder);

            stateMachine.Configure(7)
                .OnAlways(6, AddInputToValueBuilder);

            stateMachine.Configure(8)
                .OnCondition(Conditions.IsWhitespace, 11, SaveKey)
                .OnPipeSymbol(0, SaveKeyAndAddSwitchOptionAndSaveCommand)
                .OnConditionKeep(Conditions.IsValidCharacter, AddInputToValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(9)
                .OnCondition(Conditions.IsWhitespace, 2, SaveArguments)
                .OnPipeSymbol(0, SaveArgumentsAndSaveCommand)
                .OnValue('"', 15)
                .OnValueKeep(';')
                .OnEscapeSymbol(12)
                .OnCondition(Conditions.IsValidFirstCharacter, 13, AddInputToValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(10)
                .OnCondition(Conditions.IsWhitespace, 2, AddValueBuilderToArguments)
                .OnValue(';', 9, AddValueBuilderToValueArray)
                .OnPipeSymbol(0, SaveArgumentFromValueBuilderAndSaveCommand)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(11)
                .OnConditionKeep(Conditions.IsWhitespace)
                .OnPipeSymbol(0, AddSwitchOptionAndSaveCommand)
                .OnValue('"', 19)
                .OnEscapeSymbol(18)
                .OnCondition(Conditions.IsOptionNameStart, 3, AddSwitchOption)
                .OnCondition(Conditions.IsValidFirstCharacter, 17, AddInputToValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(12)
                .OnAlways(13, AddInputToValueBuilder);

            stateMachine.Configure(13)
                .OnCondition(Conditions.IsWhitespace, 2, AddArgumentToValueArrayAndSaveArguments)
                .OnPipeSymbol(0, AddArgumentToValueArrayAndSaveArgumentsAndSaveCommand)
                .OnValue(';', 9, AddValueBuilderToValueArray)
                .OnEscapeSymbol(12)
                .OnConditionKeep(Conditions.IsValidCharacter, AddInputToValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(14)
                .OnValue(';', 9, AddValueBuilderToValueArray)
                .OnPipeSymbol(0, AddArgumentToValueArrayAndSaveArgumentsAndSaveCommand)
                .OnCondition(Conditions.IsWhitespace, 2, AddArgumentToValueArrayAndSaveArguments)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(15)
                .OnValue('"', 14)
                .OnEscapeSymbol(16)
                .OnAlwaysKeep(AddInputToValueBuilder);

            stateMachine.Configure(16)
                .OnAlways(15, AddInputToValueBuilder);

            stateMachine.Configure(17)
                .OnCondition(Conditions.IsWhitespace, 2, SaveOptionFromValueBuilder)
                .OnPipeSymbol(0, SaveOptionFromValueBuilderAndSaveCommand)
                .OnEscapeSymbol(18)
                .OnValue(';', 21, AddValueBuilderToValueArray)
                .OnConditionKeep(Conditions.IsValidCharacter, AddInputToValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(18)
                .OnAlways(17, AddInputToValueBuilder);

            stateMachine.Configure(19)
                .OnEscapeSymbol(20)
                .OnValue('"', 22)
                .OnAlways(19, AddInputToValueBuilder);

            stateMachine.Configure(20)
                .OnAlways(19, AddInputToValueBuilder);

            stateMachine.Configure(21)
                .OnCondition(Conditions.IsWhitespace, 2, SaveOptionFromValueArray)
                .OnPipeSymbol(0, SaveOptionFromValueArrayAndSaveCommand)
                .OnValue('"', 26)
                .OnValueKeep(';')
                .OnEscapeSymbol(23)
                .OnCondition(Conditions.IsValidFirstCharacter, 24, AddInputToValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(22)
                .OnValue(';', 21, AddValueBuilderToValueArray)
                .OnPipeSymbol(0, SaveOptionFromValueBuilderAndSaveCommand)
                .OnCondition(Conditions.IsWhitespace, 2, SaveOptionFromValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(23)
                .OnAlways(24, AddInputToValueBuilder);

            stateMachine.Configure(24)
                .OnCondition(Conditions.IsWhitespace, 2, AddValueBuilderToValueArrayAndSaveOptionFromValueArray)
                .OnPipeSymbol(0, AddValueBuilderToValueArrayAndSaveOptionFromValueArrayAndSaveCommand)
                .OnValue(';', 21, AddValueBuilderToValueArray)
                .OnEscapeSymbol(23)
                .OnConditionKeep(Conditions.IsValidCharacter, AddInputToValueBuilder)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(25)
                .OnValue(';', 21, AddValueBuilderToValueArray)
                .OnPipeSymbol(0, AddValueBuilderToValueArrayAndSaveOptionFromValueArrayAndSaveCommand)
                .OnCondition(Conditions.IsWhitespace, 2, AddValueBuilderToValueArrayAndSaveOptionFromValueArray)
                .OnAlways(0, ProcessSyntaxError);

            stateMachine.Configure(26)
                .OnValue('"', 25)
                .OnEscapeSymbol(27)
                .OnAlwaysKeep(AddInputToValueBuilder);

            stateMachine.Configure(27)
                .OnAlways(26, AddInputToValueBuilder);

            stateMachine.OnEnding(OnEnding);
            return stateMachine;
        }

        private void OnEnding(StateMachineEndingContext<int, char> context)
        {
            if (context.Handled)
                return;

            if (context.Reason == EndingReason.EarlyStopped)
            {
                context.MarkEnd();
                return;
            }

            switch (context.State)
            {
                case 0:
                    context.MarkEnd();
                    break;
                case 1:
                    SaveCommandNameAndSaveCommand(null);
                    context.MarkEnd();
                    break;
                case 2:
                    SaveCommand(null);
                    context.MarkEnd();
                    break;
                case 3:
                    SaveCommand(null);
                    context.MarkEnd();
                    break;
                case 4:
                    AddArgumentAndSaveCommand(null);
                    context.MarkEnd();
                    break;
                case 5:
                    valueBuilder.Append('\\');
                    AddArgumentAndSaveCommand(null);
                    context.MarkEnd();
                    break;
                case 6:
                    AddArgumentAndSaveCommand(null);
                    context.MarkEnd();
                    break;
                case 7:
                    valueBuilder.Append('\\');
                    AddArgumentAndSaveCommand(null);
                    context.MarkEnd();
                    break;
                case 8:
                    SaveKeyAndAddSwitchOptionAndSaveCommand(null);
                    context.MarkEnd();
                    break;
                case 9:
                    context.FeedInputs(new char[] { ' ' });
                    break;
                case 10:
                    context.FeedInputs(new char[] { ' ' });
                    break;
                case 11:
                    AddSwitchOptionAndSaveCommand(null);
                    context.MarkEnd();
                    break;
                case 12:
                    context.FeedInputs(new char[] { '\\', ' ' });
                    break;
                case 13:
                    context.FeedInputs(new char[] { ' ' });
                    break;
                case 14:
                    context.FeedInputs(new char[] { ' ' });
                    break;
                case 15:
                    context.FeedInputs(new char[] { '"', ' ' });
                    break;
                case 16:
                    context.FeedInputs(new char[] { '\\', '"', ' ' });
                    break;
                case 17:
                    context.FeedInputs(new char[] { ' ' });
                    break;
                case 18:
                    context.FeedInputs(new char[] { '\\', ' ' });
                    break;
                case 19:
                    context.FeedInputs(new char[] { '"', ' ' });
                    break;
                case 20:
                    context.FeedInputs(new char[] { '\\', '"', ' ' });
                    break;
                case 21:
                    context.FeedInputs(new char[] { ' ' });
                    break;
                case 22:
                    context.FeedInputs(new char[] { ' ' });
                    break;
                case 23:
                    context.FeedInputs(new char[] { '\\', ' ' });
                    break;
                case 24:
                    context.FeedInputs(new char[] { ' ' });
                    break;
                case 25:
                    context.FeedInputs(new char[] { ' ' });
                    break;
                case 26:
                    context.FeedInputs(new char[] { '"', ' ' });
                    break;
                case 27:
                    context.FeedInputs(new char[] { '\\', '"', ' ' });
                    break;
            }
        }

        private void AddInputToValueBuilder(TriggeringArguments<int, char> arg) => valueBuilder.Append(arg.Input);
        private void SaveCommandName(TriggeringArguments<int, char> arg) => name = ConsumeValueBuilder();
        private void SaveCommandNameAndSaveCommand(TriggeringArguments<int, char> arg)
        {
            name = ConsumeValueBuilder();
            inputs.Add(new Input(name, new object[0], new Dictionary<string, object>()));
        }
        private void SaveNameOnlyCommand(TriggeringArguments<int, char> arg) => inputs.Add(new Input(name, new object[0], new Dictionary<string, object>()));
        private void AddArgumentAndSaveCommand(TriggeringArguments<int, char> arg)
        {
            arguments.Add(ConsumeValueBuilder());
            SaveCommand(arg);
        }
        private void SaveCommand(TriggeringArguments<int, char> arg)
        {
            inputs.Add(new Input(name, arguments.ToArray(), new Dictionary<string, object>(options)));
            arguments.Clear();
            options.Clear();
        }
        private void AddValueBuilderToValueArray(TriggeringArguments<int, char> arg) => valueArray.Add(ConsumeValueBuilder());
        private void AddValueBuilderToArguments(TriggeringArguments<int, char> arg) => arguments.Add(ConsumeValueBuilder());
        private void SaveKey(TriggeringArguments<int, char> arg) => key = ConsumeValueBuilder();
        private void SaveKeyAndAddSwitchOptionAndSaveCommand(TriggeringArguments<int, char> arg)
        {
            key = ConsumeValueBuilder();
            options[key] = true;
            inputs.Add(new Input(name, arguments.ToArray(), new Dictionary<string, object>(options)));
            arguments.Clear();
            options.Clear();
        }
        private void AddSwitchOptionAndSaveCommand(TriggeringArguments<int, char> arg)
        {
            options[key] = true;
            inputs.Add(new Input(name, arguments.ToArray(), new Dictionary<string, object>(options)));
            arguments.Clear();
            options.Clear();
        }
        private void AddSwitchOption(TriggeringArguments<int, char> arg) => options[key] = true;
        private void SaveArguments(TriggeringArguments<int, char> arg) => arguments.Add(ConsumeValueArray());
        private void SaveArgumentsAndSaveCommand(TriggeringArguments<int, char> arg)
        {
            arguments.Add(ConsumeValueArray());
            SaveCommand(arg);
        }
        private void SaveArgumentFromValueBuilderAndSaveCommand(TriggeringArguments<int, char> arg)
        {
            arguments.Add(ConsumeValueBuilder());
            SaveCommand(arg);
        }
        private void AddArgumentToValueArrayAndSaveArguments(TriggeringArguments<int, char> arg)
        {
            valueArray.Add(ConsumeValueBuilder());
            arguments.Add(ConsumeValueArray());
        }
        private void AddArgumentToValueArrayAndSaveArgumentsAndSaveCommand(TriggeringArguments<int, char> arg)
        {
            valueArray.Add(ConsumeValueBuilder());
            arguments.Add(ConsumeValueArray());
            SaveCommand(arg);
        }
        private void SaveOptionFromValueBuilder(TriggeringArguments<int, char> arg) => options[key] = ConsumeValueBuilder();
        private void SaveOptionFromValueBuilderAndSaveCommand(TriggeringArguments<int, char> arg)
        {
            options[key] = ConsumeValueBuilder();
            SaveCommand(arg);
        }
        private void SaveOptionFromValueArray(TriggeringArguments<int, char> arg) => options[key] = ConsumeValueArray();
        private void SaveOptionFromValueArrayAndSaveCommand(TriggeringArguments<int, char> arg)
        {
            options[key] = ConsumeValueArray();
            SaveCommand(arg);
        }
        private void AddValueBuilderToValueArrayAndSaveOptionFromValueArray(TriggeringArguments<int, char> arg)
        {
            valueArray.Add(ConsumeValueBuilder());
            options[key] = ConsumeValueArray();
        }
        private void AddValueBuilderToValueArrayAndSaveOptionFromValueArrayAndSaveCommand(TriggeringArguments<int, char> arg)
        {
            valueArray.Add(ConsumeValueBuilder());
            options[key] = ConsumeValueArray();
            SaveCommand(arg);
        }
        private string ConsumeValueBuilder()
        {
            string value = valueBuilder.ToString();
            valueBuilder.Clear();
            return value;
        }
        private object ConsumeValueArray()
        {
            object result;
            if (valueArray.Count == 1) result = valueArray[0];
            else result = valueArray.ToArray();
            valueArray.Clear();
            return result;
        }
        private void ProcessSyntaxError(TriggeringArguments<int, char> arg)
        {
            int index = arg.InputPosition;
            string prefix = rawInput.Trim().Substring(0, index);
            string post = rawInput.Trim().Substring(index);
            StringBuilder builder = new StringBuilder(rawInput.Length * 2);
            builder.AppendLine($"syntax error at {index}:");
            builder.AppendLine(rawInput);
            for (int i = 0; i < index; i++)
                builder.Append(' ');
            builder.Append('^');
            builder.AppendLine();
            errorMessage = builder.ToString();

            arg.FollowingAction = FollowingAction.Stop;
            arg.Handled = true;
        }
    }

    internal sealed class OutputStateMachine
    {
        public const ConsoleColor COMMAND_NAME_COLOR = ConsoleColor.White;
        public const ConsoleColor PARAMETER_NAME_COLOR = ConsoleColor.DarkGray;
        public const ConsoleColor PARAMETER_COLOR = ConsoleColor.Gray;
        public const ConsoleColor PIPE_SEPERATOR_COLOR = ConsoleColor.DarkCyan;
        public const ConsoleColor ESCAPE_COLOR = ConsoleColor.Yellow;
        public const ConsoleColor STRING_COLOR = ConsoleColor.Cyan;

        public int State => canStart ? stateMachine.State : startState;

        private IStateMachine<int, char> stateMachine;
        private bool output;
        private ConsoleColor writeColor;
        private bool canStart;
        private int startState;

        public OutputStateMachine()
        {
            stateMachine = ConfigureStateMachine();
        }

        public void InovkeOneShot(char input, ConsoleColor currentColor, out bool output, out ConsoleColor writeColor)
        {
            this.output = false;
            this.writeColor = currentColor;
            if (canStart)
            {
                stateMachine.InvokeOneShot(input);
            }
            else
            {
                stateMachine.InvokeOneShot(startState, input);
                canStart = true;
            }
            output = this.output;
            writeColor = this.writeColor;
        }
        public void ClearState()
        {
            startState = 0;
            canStart = false;
        }
        public void SetState(int state)
        {
            startState = state;
            canStart = false;
        }
        private IStateMachine<int, char> ConfigureStateMachine()
        {
            IStateMachine<int, char> machine = new StateMachine<int, char>();

            machine.Configure(0)
                .OnConditionKeep(Conditions.IsWhitespace, WriteCurrent)
                .OnCondition(Conditions.IsValidFirstCharacter, 1, WriteCommandName)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(1)
                .OnCondition(Conditions.IsWhitespace, 2, WriteCurrent)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnConditionKeep(Conditions.IsValidCharacter, WriteCommandName)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(2)
                .OnConditionKeep(Conditions.IsWhitespace, WriteCurrent)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnCondition(Conditions.IsOptionNameStart, 3, WriteParameterName)
                .OnValue('"', 6, WriteString)
                .OnEscapeSymbol(5, WriteEscape)
                .OnCondition(Conditions.IsValidFirstCharacter, 4, WriteParameter)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(3)
                .OnCondition(Conditions.IsWhitespace, 2, WriteCurrent)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnCondition(Conditions.IsValidFirstCharacter, 8, WriteParameterName)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(4)
                .OnValue(';', 9, WriteParameter)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnEscapeSymbol(5, WriteEscape)
                .OnCondition(Conditions.IsWhitespace, 2, WriteCurrent)
                .OnConditionKeep(Conditions.IsValidCharacter, WriteParameter)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(5)
                .OnAlways(4, WriteEscape);
            machine.Configure(6)
                .OnValue('"', 10, WriteString)
                .OnEscapeSymbol(7, WriteEscape)
                .OnAlwaysKeep(WriteString);
            machine.Configure(7)
                .OnAlways(6, WriteEscape);
            machine.Configure(8)
                .OnCondition(Conditions.IsWhitespace, 11, WriteCurrent)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnConditionKeep(Conditions.IsValidCharacter, WriteParameterName)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(9)
                .OnCondition(Conditions.IsWhitespace, 2, WriteCurrent)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnValue('"', 15, WriteString)
                .OnValueKeep(';', NoWrite)
                .OnEscapeSymbol(12, WriteEscape)
                .OnCondition(Conditions.IsValidFirstCharacter, 13, WriteParameter)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(10)
                .OnValue(';', 9, WriteParameter)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnCondition(Conditions.IsWhitespace, 2, WriteCurrent)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(11)
                .OnConditionKeep(Conditions.IsWhitespace, WriteCurrent)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnValue('"', 19, WriteString)
                .OnEscapeSymbol(18, WriteEscape)
                .OnCondition(Conditions.IsOptionNameStart, 3, WriteParameterName)
                .OnCondition(Conditions.IsValidFirstCharacter, 17, WriteParameter)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(12)
                .OnAlways(13, WriteEscape);
            machine.Configure(13)
                .OnCondition(Conditions.IsWhitespace, 2, WriteCurrent)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnValue(';', 9, WriteParameter)
                .OnEscapeSymbol(12, WriteEscape)
                .OnConditionKeep(Conditions.IsValidCharacter, WriteParameter)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(14)
                .OnValue(';', 9, WriteParameter)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnConditionKeep(Conditions.IsValidCharacter, WriteParameter)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(15)
                .OnValue('"', 14, WriteString)
                .OnEscapeSymbol(16, WriteEscape)
                .OnAlwaysKeep(WriteString);
            machine.Configure(16)
                .OnAlways(15, WriteEscape);
            machine.Configure(17)
                .OnCondition(Conditions.IsWhitespace, 2, WriteCurrent)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnEscapeSymbol(18, WriteEscape)
                .OnValue(';', 21, WriteParameter)
                .OnConditionKeep(Conditions.IsValidCharacter, WriteParameter)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(18)
                .OnAlways(17, WriteEscape);
            machine.Configure(19)
                .OnEscapeSymbol(20, WriteEscape)
                .OnValue('"', 22, WriteString)
                .OnAlwaysKeep(WriteString);
            machine.Configure(20)
                .OnAlways(19, WriteEscape);
            machine.Configure(21)
                .OnCondition(Conditions.IsWhitespace, 2, WriteCurrent)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnValue('"', 26, WriteString)
                .OnValueKeep(';', NoWrite)
                .OnEscapeSymbol(23, WriteEscape)
                .OnCondition(Conditions.IsValidFirstCharacter, 24, WriteParameter)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(22)
                .OnValue(';', 21, WriteParameter)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnCondition(Conditions.IsWhitespace, 2, WriteCurrent)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(23)
                .OnAlways(24, WriteEscape);
            machine.Configure(24)
                .OnCondition(Conditions.IsWhitespace, 2, WriteCurrent)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnValue(';', 21, WriteParameter)
                .OnEscapeSymbol(23, WriteEscape)
                .OnConditionKeep(Conditions.IsValidCharacter, WriteParameter)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(25)
                .OnValue(';', 21, WriteParameter)
                .OnPipeSymbol(0, WritePipeSeperator)
                .OnCondition(Conditions.IsWhitespace, 2, WriteCurrent)
                .OnAlwaysKeep(NoWrite);
            machine.Configure(26)
                .OnValue('"', 25, WriteString)
                .OnEscapeSymbol(27, WriteEscape)
                .OnAlwaysKeep(WriteString);
            machine.Configure(27)
                .OnAlways(26, WriteEscape);
            return machine;
        }

        private void NoWrite(TriggeringArguments<int, char> arg) => output = false;
        private void WriteCurrent(TriggeringArguments<int, char> arg) => output = true;
        private void WriteCommandName(TriggeringArguments<int, char> arg) => WriteWithColor(COMMAND_NAME_COLOR);
        private void WritePipeSeperator(TriggeringArguments<int, char> arg) => WriteWithColor(PIPE_SEPERATOR_COLOR);
        private void WriteParameterName(TriggeringArguments<int, char> arg) => WriteWithColor(PARAMETER_NAME_COLOR);
        private void WriteParameter(TriggeringArguments<int, char> arg) => WriteWithColor(PARAMETER_COLOR);
        private void WriteString(TriggeringArguments<int, char> arg) => WriteWithColor(STRING_COLOR);
        private void WriteEscape(TriggeringArguments<int, char> arg) => WriteWithColor(ESCAPE_COLOR);

        private void WriteWithColor(ConsoleColor color)
        {
            output = true;
            writeColor = color;
        }
    }
}
