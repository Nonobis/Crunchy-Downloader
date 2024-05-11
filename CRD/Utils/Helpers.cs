﻿using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CRD.Utils;

public class Helpers{
    /// <summary>
    /// Deserializes a JSON string into a specified .NET type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="serializerSettings">The settings for deserialization if null default settings will be used</param>
    /// <returns>The deserialized object of type T.</returns>
    public static T? Deserialize<T>(string json,JsonSerializerSettings? serializerSettings){
        try{
            return JsonConvert.DeserializeObject<T>(json,serializerSettings);
        } catch (JsonException ex){
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            throw;
        }
    }

    public static Locale ConvertStringToLocale(string? value){
        foreach (Locale locale in Enum.GetValues(typeof(Locale))){
            var type = typeof(Locale);
            var memInfo = type.GetMember(locale.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(EnumMemberAttribute), false);
            var description = ((EnumMemberAttribute)attributes[0]).Value;

            if (description == value){
                return locale;
            }
        }

        return Locale.DefaulT; // Return default if not found
    }

    public static string GenerateSessionId(){
        // Get UTC milliseconds
        var utcNow = DateTime.UtcNow;
        var milliseconds = utcNow.Millisecond.ToString().PadLeft(3, '0');

        // Get a high-resolution timestamp
        long timestamp = Stopwatch.GetTimestamp();
        double timestampToMilliseconds = (double)timestamp / Stopwatch.Frequency * 1000;
        string highResTimestamp = timestampToMilliseconds.ToString("F0").PadLeft(13, '0');

        return milliseconds + highResTimestamp;
    }

    public static async Task<(bool IsOk, int ErrorCode)> ExecuteCommandAsync(string type, string bin, string command){
        using (var process = new Process()){
            process.StartInfo.FileName = bin;
            process.StartInfo.Arguments = command;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            // To log the output or errors, you might use process.StandardOutput.ReadToEndAsync()
            // string output = await process.StandardOutput.ReadToEndAsync();
            string errors = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(errors))
                Console.WriteLine($"Error: {errors}");

            // Define success condition more appropriately based on the application
            bool isSuccess = process.ExitCode == 0;

            return (IsOk: isSuccess, ErrorCode: process.ExitCode);
        }
    }
}