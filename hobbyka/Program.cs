using Drv;
using Shared;

using var drv = await ChrDrvFactory.Create(Configuration.DrvSettings);

drv.Navigate().GoToUrl("https://nowsecure.nl");

Console.ReadKey(true);