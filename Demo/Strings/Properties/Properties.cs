// CodeContracts
// 
// Copyright (c) Microsoft Corporation
// 
// All rights reserved. 
// 
// MIT License
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// Created by Vlastimil Dort (2015-2016)
// Master thesis String Analysis for Code Contracts

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


public class Properties
{
  public static void Main()
  {
    Properties properties = new Properties();

    properties.Url("http://www.example.com");
    properties.Number("1066");
    properties.FileExtension("image.jpg");
    properties.ForbiddenCharacters("allowed characters");
    properties.ForbiddenCharacters("");
    properties.Date("2016-04-21");
    properties.Guid("5b2d921e-061d-4460-a5df-4f337645851f");
    properties.Email("no.user@example.com");
  }

  public string Url(string value)
  {
    Contract.Requires(value.StartsWith("http://", StringComparison.Ordinal));
    Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
    Contract.Ensures(Contract.Result<string>().StartsWith("http://", StringComparison.Ordinal));

    return value;
  }

  public string Number(string value)
  {
    Contract.Requires(Regex.IsMatch(value, "^[0-9]+\\z"));
    Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
    Contract.Ensures(Regex.IsMatch(Contract.Result<string>(), "^[0-9]*\\z"));
    Contract.Ensures(Regex.IsMatch(Contract.Result<string>(), "^[0-9]+\\z"));

    return value;
  }

  public string FileExtension(string value)
  {
    Contract.Requires(value.EndsWith(".jpg", StringComparison.Ordinal));
    Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
    Contract.Ensures(Contract.Result<string>().EndsWith(".jpg", StringComparison.Ordinal));

    return value;
  }

  public string ForbiddenCharacters(string value)
  {
    Contract.Requires(!value.Contains("\'"));
    Contract.Requires(!value.Contains("\\"));
    Contract.Requires(!value.Contains("\""));

    Contract.Ensures(!Regex.IsMatch(Contract.Result<string>(), "[\"\']"));

    Contract.Ensures(!Contract.Result<string>().Contains("\'"));
    Contract.Ensures(!Contract.Result<string>().Contains("\\"));
    Contract.Ensures(!Contract.Result<string>().Contains("\""));

    return value;
  }

  public string Date(string value)
  {
    Contract.Requires(Regex.IsMatch(value, "^[0-9]{4}-[0-9]{2}-[0-9]{2}\\z"));
    Contract.Ensures(Regex.IsMatch(Contract.Result<string>(), "^[0-9-]*\\z"));
    Contract.Ensures(Regex.IsMatch(Contract.Result<string>(), "^[0-9]{4}-[0-9]{2}-[0-9]{2}\\z"));
    Contract.Ensures(Contract.Result<string>().Contains("-"));

    return value;
  }

  public string Guid(string value)
  {
    Contract.Requires(Regex.IsMatch(value, "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\\z"));
    Contract.Ensures(Regex.IsMatch(Contract.Result<string>(), "^[0-9a-fA-F-]*\\z"));
    Contract.Ensures(Regex.IsMatch(Contract.Result<string>(), "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\\z"));

    return value;
  }


  public string Email(string value)
  {
    Contract.Requires(Regex.IsMatch(value, "^[a-z0-9_]+(?:.[a-z0-9_]+)*@[a-z0-9_]+(?:.[a-z0-9_]+)+\\z"));

    Contract.Ensures(Regex.IsMatch(Contract.Result<string>(), "^[0-9a-zA-Z._@-]*\\z"));
    Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
    Contract.Ensures(Regex.IsMatch(Contract.Result<string>(), "^[a-z0-9_]+(?:.[a-z0-9_]+)*@[a-z0-9_]+(?:.[a-z0-9_]+)+\\z"));
    Contract.Ensures(Contract.Result<string>().Contains("@"));

    return value;
  }
}
