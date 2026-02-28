using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace FamilyFinance;

class Program : global::Microsoft.Maui.MauiTizenApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    static void Main(string[] args)
    {
        var app = new Program();
        app.Run(args);
    }
}
