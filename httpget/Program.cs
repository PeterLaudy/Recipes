var request = new HttpClient();
Console.Out.Write(await request.GetStringAsync(args[0]));