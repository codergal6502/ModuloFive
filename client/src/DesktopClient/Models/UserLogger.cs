// using System;
// using System.Collections.Generic;
// using System.Collections.ObjectModel;

// namespace ModuloFivePadClient.DesktopClient.Models
// {
//     public class UserLogger : IUserLogger
//     {
//         private readonly Dictionary<DateTime, string> _internalDictionary;

//         public UserLogger()
//         {
//             this._internalDictionary = new Dictionary<DateTime, string>();
//         }

//         public ReadOnlyDictionary<DateTime, string> LogMessages => new(_internalDictionary);

//         public void LogMessage(string format, params object[] args)
//         {
//             _internalDictionary[DateTime.Now] = String.Format(format: format, args: args);
//         }
//     }
// }