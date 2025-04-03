using System;
using System.Runtime.InteropServices;

namespace Projet_PSI
{
    internal class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [STAThread]
        static void Main(string[] args)
        {
            AllocConsole();
            MenuPrincipal.Lancer();
        }
    }
}