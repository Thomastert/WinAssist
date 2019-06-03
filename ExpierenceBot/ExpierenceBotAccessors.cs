// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;

namespace ExpierenceBot
{

    public class ExpierenceBotAccessors
    {
        /// Initializes a new instance of the class.
        public ExpierenceBotAccessors(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }
        
        /// <remarks>Accessors require a unique name.</remarks>
        /// <value>The accessor name for the counter accessor.</value>
        public static string CounterStateName { get; } = $"{nameof(ExpierenceBotAccessors)}.CounterState";

        public static string WelcomeStateName { get; } = $"{nameof(ExpierenceBotAccessors)}.WelcomeState";


        public IStatePropertyAccessor<CounterState> CounterState { get; set; }

        public IStatePropertyAccessor<WelcomeState> WelcomeState { get; set; }


        public ConversationState ConversationState { get; }
    }
}
