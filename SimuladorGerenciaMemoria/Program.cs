using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimuladorGerenciaMemoria.Scripts;
using SimuladorGerenciaMemoria.Models;

namespace SimuladorGerenciaMemoria
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            // testes para geração do script
            Memory m = new Memory();
            m.ID = 1;
            m.Name = "Memoria 1";
            m.Size = 100000;
            m.FramesSize = 1000;
            m.FramesQTD = m.Size / m.FramesSize;

            ScriptProcess p = new ScriptProcess(m,100);
            p.CreateProcesses();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
