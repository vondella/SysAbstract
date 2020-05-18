using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Sys.Jobs
{
    public class BackgroundWorker
    {
        private readonly Serilog.ILogger _logger;
        private int _counter;

        public BackgroundWorker(Serilog.ILogger logger)
        {
            _counter = 0;
            _logger = logger;
        }

        public void Execute()
        {
            //_logger.LogDebug(_counter.ToString());
            _counter++;
        }
    }
}
