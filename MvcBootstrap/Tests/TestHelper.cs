namespace MvcBootstrap.Tests
{
    using System;

    public static class TestHelper
    {
        /// <summary>
        /// For non-MVC projects, we must set up "AttachDBFilename=|DataDirectory|..." to work ourselves.
        /// </summary>
        public static void SetDataDirectoryToAppDirectory()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
