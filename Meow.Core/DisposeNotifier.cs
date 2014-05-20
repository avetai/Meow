using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Core
{
    public class DisposeNotifier : IDisposable
    {
	    private readonly Action _executeOnDispose;
	    private bool _disposed;

	    public DisposeNotifier(Action executeOnDispose)
	    {
		    if (executeOnDispose == null)
		    {
			    throw new ArgumentNullException("executeOnDispose");
		    }
		    
			_executeOnDispose = executeOnDispose;
	    }

	    public void Dispose()
	    {
		    if (_disposed)
		    {
			    return;
		    }

		    _executeOnDispose();

		    _disposed = true;
	    }
    }
}
