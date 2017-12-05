# Needed to start debugger for flask app in Visual Studio Code IDE
import sys
import re
from flask.cli import main
cmd = re.sub(r'(-script\.pyw|\.exe)?$', '', sys.argv[0])
sys.argv[0] = cmd
sys.exit(main())