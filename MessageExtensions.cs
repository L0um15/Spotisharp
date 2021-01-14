using System;
using System.Collections.Generic;
using System.Text;

namespace SpotiSharp
{
    static class MessageExtensions
    {
        public static string MoveToRight(this string message)
        {
            Console.CursorLeft = Console.BufferWidth - message.Length;
            return message;
        }
        public static string Truncate(this string message)
        {
            int MaxWidth = Console.BufferWidth - 24;
            if (string.IsNullOrEmpty(message)) return message;
            return message.Length <= MaxWidth ? message : message.Substring(0, MaxWidth) + "...";
        }
    }
}