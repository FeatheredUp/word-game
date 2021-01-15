// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Controllers.GameController.Create(System.String)~Words.API.ViewModels.CreateResult")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Controllers.GameController.Join(System.String,System.String)~Words.API.ViewModels.JoinResult")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Controllers.GameController.Creating(System.String,System.String)~Words.API.ViewModels.CreatingResult")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Controllers.GameController.Wait(System.String,System.String)~Words.API.ViewModels.WaitResult")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Controllers.GameController.Play(Words.API.ViewModels.PlayInput)~Words.API.ViewModels.PlayResult")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Controllers.GameController.Start(System.String,System.String,System.String)~Words.API.ViewModels.StartResult")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Controllers.GameController.Pass(System.String,System.String)~Words.API.ViewModels.PassResult")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Controllers.GameController.Swap(System.String,System.String,System.String)~Words.API.ViewModels.SwapResult")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Controllers.GameController.TryPlay(Words.API.ViewModels.PlayInput)~Words.API.ViewModels.TryPlayResult")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Controllers.GameController.History(System.String)~Words.API.ViewModels.HistoryResult")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Logic.PlayChecker.#ctor(Words.API.DataModels.Player,Words.API.DataModels.GameState,System.Collections.Generic.List{Words.API.DataModels.TilePlacement},Words.API.Repository.IRepository)")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.EmptyRackException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.FullRackException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.GameAlreadyInProgressException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.GameDoesNotExistException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.GameNotStartedException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.InvalidGameIdException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.InvalidPlayerNameException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.LetterNotOnRackException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.NoMoreLettersException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.NotEnoughPlayersException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.PlayerAlreadyInGameException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.PlayerNotInGameException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.UnexpectedPlayerException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.ValidationException.#ctor")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Logic.GameLogic.Swap(Words.API.DataModels.GameId,Words.API.DataModels.PlayerId,System.String)~Words.API.DataModels.GameState")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Exceptions.GameAtCapacityException.#ctor")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Startup.ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>", Scope = "member", Target = "~M:Words.API.Startup.Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder,Microsoft.AspNetCore.Hosting.IWebHostEnvironment)")]
