//---------------------------------------------------------------------------
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
//
//---------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Security;
using System.Security.Permissions;
using System.ComponentModel;

namespace RS.Widgets.Controls
{
    public static class CommandHelpers
    {
        private const char MODIFIERS_DELIMITER = '+';
        public const char DISPLAYSTRING_SEPARATOR = ',';
        public static TypeConverter _keyGestureConverter = new KeyGestureConverter();
        // Lots of specialized registration methods to avoid new'ing up more common stuff (like InputGesture's) at the callsite, as that's frequently
        // repeated and increases code size.  Do it once, here.  



        public static void RegisterCommandHandler(Type controlType,
            RoutedCommand command, 
            ExecutedRoutedEventHandler executedRoutedEventHandler)
        {
            PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, null, null);
        }

        public static void RegisterCommandHandler(Type controlType,
            RoutedCommand command, 
            ExecutedRoutedEventHandler executedRoutedEventHandler,
            InputGesture inputGesture)
        {
            PrivateRegisterCommandHandler(controlType, command,
                executedRoutedEventHandler, 
                null, inputGesture);
        }

        public static void RegisterCommandHandler(Type controlType,
            RoutedCommand command, 
            ExecutedRoutedEventHandler executedRoutedEventHandler,
            Key key)
        {
            PrivateRegisterCommandHandler(controlType,
                command,
                executedRoutedEventHandler, 
                null, 
                new KeyGesture(key));
        }

        public static void RegisterCommandHandler(Type controlType,
            RoutedCommand command, 
            ExecutedRoutedEventHandler executedRoutedEventHandler,
            InputGesture inputGesture, InputGesture inputGesture2)
        {
            PrivateRegisterCommandHandler(controlType,
                command, executedRoutedEventHandler, null,
                inputGesture, inputGesture2);
        }

        public static void RegisterCommandHandler(Type controlType,
            RoutedCommand command,
            ExecutedRoutedEventHandler executedRoutedEventHandler,
            CanExecuteRoutedEventHandler canExecuteRoutedEventHandler)
        {
            PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, canExecuteRoutedEventHandler, null);
        }

        public static void RegisterCommandHandler(Type controlType,
            RoutedCommand command,
            ExecutedRoutedEventHandler executedRoutedEventHandler,
            CanExecuteRoutedEventHandler canExecuteRoutedEventHandler, InputGesture inputGesture)
        {
            PrivateRegisterCommandHandler(controlType,
                command, 
                executedRoutedEventHandler,
                canExecuteRoutedEventHandler, 
                inputGesture);
        }

        public static void RegisterCommandHandler(Type controlType, 
            RoutedCommand command, 
            ExecutedRoutedEventHandler executedRoutedEventHandler,
            CanExecuteRoutedEventHandler canExecuteRoutedEventHandler, Key key)
        {
            PrivateRegisterCommandHandler(controlType, 
                command, 
                executedRoutedEventHandler,
                canExecuteRoutedEventHandler, new KeyGesture(key));
        }

        public static void RegisterCommandHandler(Type controlType,
            RoutedCommand command, 
            ExecutedRoutedEventHandler executedRoutedEventHandler,
            CanExecuteRoutedEventHandler canExecuteRoutedEventHandler,
            InputGesture inputGesture, InputGesture inputGesture2)
        {
            PrivateRegisterCommandHandler(controlType,
                command,
                executedRoutedEventHandler, 
                canExecuteRoutedEventHandler,
                inputGesture,
                inputGesture2);
        }

        public static void RegisterCommandHandler(Type controlType,
            RoutedCommand command, 
            ExecutedRoutedEventHandler executedRoutedEventHandler,
            CanExecuteRoutedEventHandler canExecuteRoutedEventHandler,
            InputGesture inputGesture,
            InputGesture inputGesture2,
            InputGesture inputGesture3,
            InputGesture inputGesture4)
        {
            PrivateRegisterCommandHandler(controlType,
                command, 
                executedRoutedEventHandler, 
                canExecuteRoutedEventHandler,
                inputGesture,
                inputGesture2,
                inputGesture3,
                inputGesture4);
        }

        public static void RegisterCommandHandler(Type controlType,
            RoutedCommand command, 
            Key key,
            ModifierKeys modifierKeys,
            ExecutedRoutedEventHandler executedRoutedEventHandler,
            CanExecuteRoutedEventHandler canExecuteRoutedEventHandler)
        {
            PrivateRegisterCommandHandler(controlType, command, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(key, modifierKeys));
        }

        public static void RegisterCommandHandler(Type controlType, RoutedCommand command, ExecutedRoutedEventHandler executedRoutedEventHandler,
                                                    string srid1, string srid2)
        {
            PrivateRegisterCommandHandler(controlType,
                command,
                executedRoutedEventHandler, 
                null,
                CreateFromResourceStrings(srid1, srid2));
        }

        public static void RegisterCommandHandler(Type controlType,
            RoutedCommand command,
            ExecutedRoutedEventHandler executedRoutedEventHandler,
            CanExecuteRoutedEventHandler canExecuteRoutedEventHandler, 
            string srid1, 
            string srid2)
        {
            PrivateRegisterCommandHandler(controlType,
                command,
                executedRoutedEventHandler, 
                canExecuteRoutedEventHandler,
                CreateFromResourceStrings(srid1, srid2));
        }

        public static KeyGesture CreateFromResourceStrings(string keyGestureToken,
            string keyDisplayString)
        {
            // combine the gesture and the display string, producing a string
            // that the type converter will recognize
            if (!String.IsNullOrEmpty(keyDisplayString))
            {
                keyGestureToken +=DISPLAYSTRING_SEPARATOR + keyDisplayString;
            }

            return _keyGestureConverter.ConvertFromInvariantString(keyGestureToken) as KeyGesture;
        }

        // 'params' based method is private.  Call sites that use this bloat unwittingly due to implicit construction of the params array that goes into IL.
        public static void PrivateRegisterCommandHandler(Type controlType,
            RoutedCommand command,
            ExecutedRoutedEventHandler executedRoutedEventHandler,
            CanExecuteRoutedEventHandler canExecuteRoutedEventHandler, 
            params InputGesture[] inputGestures)
        {
            // Validate parameters
            Debug.Assert(controlType != null);
            Debug.Assert(command != null);
            Debug.Assert(executedRoutedEventHandler != null);
            // All other parameters may be null

            // Create command link for this command
            CommandManager.RegisterClassCommandBinding(controlType, new CommandBinding(command, executedRoutedEventHandler, canExecuteRoutedEventHandler));

            // Create additional input binding for this command
            if (inputGestures != null)
            {
                for (int i = 0; i < inputGestures.Length; i++)
                {
                    CommandManager.RegisterClassInputBinding(controlType, new InputBinding(command, inputGestures[i]));
                }
            }
            
        }

        public static bool CanExecuteCommandSource(ICommandSource commandSource)
        {
            ICommand command = commandSource.Command;
            if (command != null)
            {
                object parameter = commandSource.CommandParameter;
                IInputElement target = commandSource.CommandTarget;

                RoutedCommand routed = command as RoutedCommand;
                if (routed != null)
                {
                    if (target == null)
                    {
                        target = commandSource as IInputElement;
                    }
                    return routed.CanExecute(parameter, target);
                }
                else
                {
                    return command.CanExecute(parameter);
                }
            }

            return false;
        }
   
     
        // This allows a caller to override its ICommandSource values (used by Button and ScrollBar)
        public static void ExecuteCommand(ICommand command, 
            object parameter, 
            IInputElement target)
        {
            RoutedCommand routed = command as RoutedCommand;
            if (routed != null)
            {
                if (routed.CanExecute(parameter, target))
                {
                    routed.Execute(parameter, target);
                }
            }
            else if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }
}
