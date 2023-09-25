using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace WindowsAudioSession.Helpers
{
    public static class NetworkHelper
    {
        public static string DownloadUrl
        {
            get => "https://github.com/KRtekTM/AudioSpectrumVisualizer/releases/latest";
        }
        public static Version CurrentVersion
        {
            get
            {
                // Získání informací o aktuálním assembly (EXE)
                Assembly assembly = Assembly.GetEntryAssembly();

                // Získání verze z aktuální assembly
                return assembly.GetName().Version;
            }
        }
        public static KeyValuePair<bool, Version> CheckUpdate()
        {
            try
            {
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(DownloadUrl);
                    HttpWebResponse response;
                    string resUri;

                    response = (HttpWebResponse)req.GetResponse();
                    resUri = response.ResponseUri.AbsoluteUri;

                    Version latest = new Version(resUri.Substring(resUri.LastIndexOf("/") + 1).Replace("v", ""));

                    return new KeyValuePair<bool, Version>(latest.CompareTo(CurrentVersion) > 0, latest);
                }
                else
                {
                    return new KeyValuePair<bool, Version>(false, CurrentVersion);
                }
            }
            catch
            {
                return new KeyValuePair<bool, Version>(false, CurrentVersion);
            }
        }

    }
}
