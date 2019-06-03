// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;


namespace ExpierenceBot
{
    /// <summary>
    /// Represents a bot that processes incoming activities. Talk bot.
    /// each interaction an instance of this class is created and the OnTurnAsync method is called. 
    /// Transient lifetime services are created each time they're requested. 
    /// For each Activity received a new instance of this class is created. meaning no big things that take longer then a "turn"
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class ExpierenceBotBot : IBot
    {
        private readonly ExpierenceBotAccessors _accessors;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1#windows-eventlog-provider"/>
        public ExpierenceBotBot(ExpierenceBotAccessors accessors, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<ExpierenceBotBot>();
            _logger.LogTrace("Turn start.");
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
        }

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        /// 

        // my mess
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types

            var startState = await _accessors.WelcomeState.GetAsync(turnContext, () => new WelcomeState());


            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (startState.DidBotWelcomeUser == false)
                {
                    var state = await _accessors.CounterState.GetAsync(turnContext, () => new CounterState());

                    // Bump the turn count for this conversation.
                    state.TurnCount++;

                    // Set the property using the accessor.
                    await _accessors.CounterState.SetAsync(turnContext, state);

                    // Save the new turn count into the conversation state.
                    await _accessors.ConversationState.SaveChangesAsync(turnContext);
                    startState.Username = turnContext.Activity.Text;
                    
                    await turnContext.SendActivityAsync($"Good to see you again {startState.Username}\n");
                    //put to true
                    startState.DidBotWelcomeUser = true;
                    await _accessors.WelcomeState.SetAsync(turnContext, startState);
                    await _accessors.ConversationState.SaveChangesAsync(turnContext);
                }
                else
                {
                    var _genericMessage = "i am bot and do not understand this yet";
                    var text = turnContext.Activity.Text.ToLowerInvariant();
                    switch (text)
                    {       // welcome
                        case "hello":
                        case "hi":
                            await turnContext.SendActivityAsync($"Hi {startState.Username}.", cancellationToken: cancellationToken);
                            break;
                            // response
                        case "intro":
                        case "help":
                            await turnContext.SendActivityAsync($"coming soon.", cancellationToken: cancellationToken);
                            break;
                            //launch application on call
                        case "unity":
                            var unity = System.Diagnostics.Process.Start(@"D:\unity\Editor\Unity.exe");
                            unity.Start();
                            
                            break;
                            //image attachment
                        case "christmas":
                            var reply = turnContext.Activity.CreateReply();
                            var imagePath = Path.Combine(Environment.CurrentDirectory, @"assets\Christmas.jpg");
                            var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));
                            var attachment = new Attachment
                            {
                                Name = @"assets\Christmas.jpg",
                                ContentUrl = $"data:image/png;base64,{imageData}",
                                ContentType = "image/jpg",
                                
                            };
                            reply.Attachments = new List<Attachment>() { attachment };
                            await turnContext.SendActivityAsync(reply, cancellationToken);
                            break;
                            // herocard mood meter
                        case "mood":
                            var anwser = turnContext.Activity.CreateReply();
                            var card = new HeroCard
                            {
                                Text = "How is your day?",
                                Buttons = new List<CardAction>()
                                {
                                new CardAction(ActionTypes.PostBack, title: "1. Good!", value: "moodresponse1yareyare"),
                                new CardAction(ActionTypes.PostBack, title: "2. Okay", value: "moodresponse2yareyare"),
                                new CardAction(ActionTypes.PostBack, title: "3. Not so good :(", value: "moodresponse3yareyare"),

                                }
                            };
                            anwser.Attachments = new List<Attachment>() { card.ToAttachment() };
                            await turnContext.SendActivityAsync(anwser, cancellationToken);
                            break;
                            // herocard mood meter answers
                        case "moodresponse1yareyare":
                            await turnContext.SendActivityAsync($"Great keep it up!", cancellationToken: cancellationToken);
                            break;

                        case "moodresponse2yareyare":
                            await turnContext.SendActivityAsync($"Well it could be worse keep at it!", cancellationToken: cancellationToken);
                            break;

                        case "moodresponse3yareyare":
                            var reply3 = turnContext.Activity.CreateReply();
                            var imagePath3 = Path.Combine(Environment.CurrentDirectory, @"assets\ganbatte.png");
                            var imageData3 = Convert.ToBase64String(File.ReadAllBytes(imagePath3));
                            var attachment3 = new Attachment
                            {
                                Name = @"assets\ganbatte.png",
                                ContentUrl = $"data:image/png;base64,{imageData3}",
                                ContentType = "image/jpg",

                            };
                            reply3.Attachments = new List<Attachment>() { attachment3 };
                            await turnContext.SendActivityAsync(reply3, cancellationToken);
                            break;

                        case "status":
                            await turnContext.SendActivityAsync($"Date " + DateTime.Now, cancellationToken: cancellationToken);
                            var file = File.Create(@"assets\status\newtxt.txt");
                            // write to file test
                            break;

                        case "music":
                            var response = turnContext.Activity.CreateReply();
                            var musicCard = new HeroCard
                            {
                                Text = "Choose your music",
                                Buttons = new List<CardAction>()
                                {
                                 new CardAction(ActionTypes.OpenUrl, title: "NewVegas", value: $"https://www.youtube.com/watch?v=DyY9Wpfajqo"),
                                 new CardAction(ActionTypes.OpenUrl, title: "Anime", value: $"https://www.youtube.com/watch?v=5_iuNaULT5k&list=PLNfpdQIhS1GZVvoSc_M-X9_NS-d4XWTmO&index=8&t=0s"),
                                 new CardAction(ActionTypes.OpenUrl, title: "MyHits", value: $"https://www.youtube.com/watch?v=PXjjVnqS-7c&list=PLNfpdQIhS1GZSHbUJFnbM4A0mxapwHHZa&index=2&t=0s"),
                                 //auto play function
                                }
                            };
                            response.Attachments = new List<Attachment>() { musicCard.ToAttachment() };
                            await turnContext.SendActivityAsync(response, cancellationToken);
                            
                            break;
                    
                            // default answer
                        default:
                            await turnContext.SendActivityAsync(_genericMessage, cancellationToken: cancellationToken);
                            break;
                    }
                }
            }
            else
            {
                // Default behaivor for all other type of events.
                var ev = turnContext.Activity.AsEventActivity();
                //await turnContext.SendActivityAsync($"Received event: {ev.Name}");
                await turnContext.SendActivityAsync($"Please give me your name.");
            }
            // save any state changes made to your state objects.
            await _accessors.ConversationState.SaveChangesAsync(turnContext);
        }
    }
}







        /*


           if (startState.DidBotWelcomeUser == true)
           { 
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                
                // Get the conversation state from the turn context.
                var state = await _accessors.CounterState.GetAsync(turnContext, () => new CounterState());

                // Bump the turn count for this conversation.
                state.TurnCount++;

                // Set the property using the accessor.
                await _accessors.CounterState.SetAsync(turnContext, state);

                // Save the new turn count into the conversation state.
                await _accessors.ConversationState.SaveChangesAsync(turnContext);

                // Echo back to the user whatever they typed.
                var responseMessage = $"Line {state.TurnCount}: Unknown Command '{turnContext.Activity.Text}'\n";
                await turnContext.SendActivityAsync(responseMessage);
                }

                else
                {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
                }
           }
           else
           {
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    var state = await _accessors.CounterState.GetAsync(turnContext, () => new CounterState());

                    // Bump the turn count for this conversation.
                    state.TurnCount++;

                    // Set the property using the accessor.
                    await _accessors.CounterState.SetAsync(turnContext, state);

                    // Save the new turn count into the conversation state.
                    await _accessors.ConversationState.SaveChangesAsync(turnContext);

                    var userName = turnContext.Activity.Text;
                    await turnContext.SendActivityAsync($"Good to see you again {userName}\n");
                    //put to true
                    startState.DidBotWelcomeUser = true;
                    await _accessors.WelcomeState.SetAsync(turnContext, startState);
                    await _accessors.ConversationState.SaveChangesAsync(turnContext);
                }
                else
                {
                    await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
                }
           } */
