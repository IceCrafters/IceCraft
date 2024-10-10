// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime;

using IceCraft.Extensions.CentralRepo.Api;
using Jint;
using Jint.Native;

/// <summary>
/// Implements string formatting similar to Node.js <see href="https://nodejs.org/api/util.html#utilformatformat-args"><c>util.format</c></see>.
/// </summary>
public class MashiroFormatter
{
    public const char FormatSpecifier = '%';
    public const char StringFormat = 's';
    public const char NumberFormat = 'd';
    public const char CssFormat = 'c';

    internal static void ProcessPlaceholder(TextWriter to, 
        char specifier, 
        JsValue[] args, 
        ref int currentArg)
    {
        switch (specifier)
        {
            // '%%': Write %
            case FormatSpecifier:
                to.Write(FormatSpecifier);
                break;
            // '%s': ToString
            case StringFormat:
                WriteStringFormat(currentArg);
                currentArg++;
                break;
            // '%d': Number format
            case NumberFormat:
                WriteNumberFormat(currentArg);
                currentArg++;
                break;
            // '%c': Ignore CSS
            case CssFormat:
                currentArg++;
                break;
            default:
                throw MashiroFormatError.UnknownSpecifier(specifier);
        }

        void EnsurePos(int position)
        {
            if (args.Length <= position)
            {
                throw MashiroFormatError.CreateNotEnoughArguments(position, args.Length);
            }
        }

        void WriteStringFormat(int position)
        {
            EnsurePos(position);
            to.Write(args[position].ToString());
        }

        void WriteNumberFormat(int position)
        {
            EnsurePos(position);
            var value = args[position];
            if (!value.IsNumber())
            {
                throw MashiroFormatError.NotNumber(position);
            }

            to.Write(value.AsNumber().ToString());
        }
    }

    public static void Format(TextWriter objective, string template, params JsValue[] args)
    {
        if (args.Length == 0)
        {
            objective.Write(template);
            return;
        }

        var reader = new StringReader(template);
        var inExpression = false;
        var specifier = 'x';
        var currentArg = 0;

        while (true)
        {
            var read = reader.Read();
            if (read == -1)
            {
                break;
            }

            var ch = (char)read;
            if (!inExpression)
            {
                // If not in expression and is not format specifier,
                // write to objective
                if (ch != FormatSpecifier)
                {
                    objective.Write(ch);
                    continue;
                }

                // If ch IS format specifier...
                inExpression = true;
                continue;
            }

            if (specifier == 'x')
            {
                specifier = ch;
            }

            ProcessPlaceholder(objective, specifier, args, ref currentArg);
            specifier = 'x';
            inExpression = false;
        }

        // Disallow cases where format placeholder did not end
        // (for example, "string %")
        if (inExpression)
        {
            throw new MashiroFormatError("Format placeholder did not properly end");
        }
    }

    public static string Format(string template, params JsValue[] args)
    {
        using var writer = new StringWriter();
        Format(writer, template, args);

        return writer.ToString();
    }
}
