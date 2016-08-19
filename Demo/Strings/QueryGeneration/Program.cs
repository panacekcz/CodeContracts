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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;

class QueryGenerator
{
  [ContractInvariantMethod]
  private void Invariant()
  {
    Contract.Invariant(Regex.IsMatch(email, "^[a-z0-9_]+(?:.[a-z0-9_]+)*@[a-z0-9_]+(?:.[a-z0-9_]+)+\\z"));
    Contract.Invariant(amount > 0);
    Contract.Invariant(image.EndsWith(".jpg", System.StringComparison.Ordinal));
    Contract.Invariant(!image.Contains("\'"));
  }

  public QueryGenerator()
  {
    this.email = "none@none.com";
    this.amount = 1;
    this.image = "default.jpg";
  }

  private string email;

  public string Email
  {
    get
    {
      Contract.Ensures(Regex.IsMatch(Contract.Result<string>(), "^[a-z0-9_]+(?:.[a-z0-9_]+)*@[a-z0-9_]+(?:.[a-z0-9_]+)+\\z"));
      return email;
    }
    set
    {
      Contract.Requires(Regex.IsMatch(value, "^[a-z0-9_]+(?:.[a-z0-9_]+)*@[a-z0-9_]+(?:.[a-z0-9_]+)+\\z"));
      email = value;
    }
  }

  private int amount;

  public int Amount
  {
    get
    {
      Contract.Ensures(Contract.Result<int>() > 0);
      return amount;
    }
    set
    {
      Contract.Requires(value > 0);
      amount = value;
    }
  }

  private string image;

  public string Image
  {
    get
    {
      Contract.Ensures(Contract.Result<string>().EndsWith(".jpg", System.StringComparison.Ordinal));
      return image;
    }
    set
    {
      Contract.Requires(value.EndsWith(".jpg", System.StringComparison.Ordinal));

      if (image.Contains("\'"))
      {
        throw new ArgumentException();
      }

      image = value;
    }
  }

  private DateTime date;

  public DateTime Date
  {
    get
    {
      return date;
    }

    set
    {
      date = value;
    }
  }

  public string GenerateString()
  {
    Contract.Ensures(Contract.Result<string>().StartsWith("SELECT", StringComparison.Ordinal));

    string amountString = Amount.ToString();
    Contract.Assert(Regex.IsMatch(amountString, "^\\d+\\z"));

    string dateString = Date.Date.ToString("yyyy-MM-dd");

    Contract.Assume(Regex.IsMatch(dateString, "^[0-9]{4}-[0-9]{2}-[0-9]{2}\\z"));
    Contract.Assert(!dateString.Contains('\''));

    string sqlQuery = "SELECT Product, Date, Amount FROM Orders WHERE Amount > ";
    sqlQuery += amountString;
    sqlQuery += " AND Date < '";
    sqlQuery += dateString;
    sqlQuery += "'";

    if (email != null)
    {
      Contract.Assert(!email.Contains("\'"));
      sqlQuery += " AND User='";
      sqlQuery += email;
      sqlQuery += "'";
    }

    Contract.Assert(!image.Contains("\'"));
    sqlQuery += " AND Image='" + Image + "'";

    return sqlQuery;
  }
}
public static class Program
{
  private static string GetImage(string image)
  {
    Contract.Ensures(Contract.Result<string>().EndsWith(".jpg", StringComparison.Ordinal));

    string s;

    if (string.IsNullOrEmpty(image))
    {
      s = "none.jpg";
    }
    else if (image.EndsWith(".jpg", StringComparison.Ordinal))
    {
      s = image;
    }
    else
    {
      s = image + "_image.jpg";
    }
    return s;
  }


  public static void Main()
  {
    string image = Console.ReadLine();

    QueryGenerator s = new QueryGenerator();
    s.Email = "abc@def.com";
    s.Image = GetImage(image);
    s.Amount = 100;
    s.Date = new DateTime(2015, 01, 01);
    string query = s.GenerateString();

    Console.WriteLine(query);
    Console.ReadKey();
  }
}
