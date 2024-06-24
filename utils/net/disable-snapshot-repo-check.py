# Replaces the snapshot repo check in .net ps1 scripts to not exit immediately. 

import sys

if len(sys.argv) != 2:
    print("Usage: python disable-snapshot-repo-check.py <abs-file-path>")
    sys.exit(1)

# Read the file content
with open(sys.argv[1], 'r') as file:
  filedata = file.read()

print("Old content: ", filedata)

# Replace the target string
filedata = filedata.replace('Die "Error: could not download metadata from Maven ($($response2.StatusCode))"', 'Write-Output "Error: could not download metadata from Maven ($($response2.StatusCode))"\n        return')

print("New content: ", filedata)

# Write the file out again
with open(sys.argv[1], 'w') as file:
  file.write(filedata)

sys.exit(0)