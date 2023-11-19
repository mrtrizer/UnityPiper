using System;
using System.IO;
using System.Threading.Tasks;

namespace Abuksigun.Piper
{
	public sealed unsafe class Piper : IDisposable
	{
		PiperLib.PiperConfig* config;
		public PiperLib.PiperConfig* Config => config;

		Piper(PiperLib.PiperConfig* config)
		{
            this.config = config;
        }

		~Piper()
		{
            Dispose();
        }

		public void Dispose()
		{
			if (config != null)
			{
                PiperLib.terminatePiper(config);
                PiperLib.destroy_PiperConfig(config);
                config = null;
            }
		}

		public static Task<Piper> LoadPiper(string fullEspeakDataPath)
		{
            if (!Directory.Exists(fullEspeakDataPath))
                throw new DirectoryNotFoundException("Espeak data directory not found");

            return Task.Run(() =>
			{
				var piperConfig = PiperLib.create_PiperConfig(fullEspeakDataPath);
				try
				{
					PiperLib.initializePiper(piperConfig);
					return Task.FromResult(new Piper(piperConfig));
				}
				catch
				{
					PiperLib.destroy_PiperConfig(piperConfig);
					throw;
				}
			});
		}
    }
}