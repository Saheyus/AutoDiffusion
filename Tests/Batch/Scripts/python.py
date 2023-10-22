import time
import sys
import json
import pprint
print('Pyton starting...')
print ('Argument List:', str(sys.argv))
time.sleep(0)
print('Pyton stopping...')

result = {
  "images": [
    "model.png",
    "model2.png"
  ]
}

y = json.dumps(result)
print(y)
sys.exit(0);