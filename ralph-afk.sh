#!/bin/bash
set -e

if [ -z "$1" ]; then
  echo "Usage: $0 <iterations>"
  exit 1
fi

for ((i=1; i<=$1; i++)); do
  result=$(docker sandbox run codex-demo -- --sandbox danger-full-access ".scratch\deepening-6-value-types\PRD.md \
  1. Find the highest-priority issue and implement it. \
  2. Run your tests and type checks. \
  3. Update the PRD with what was done. \
  4. Update the issue file status when you're done. \
  5. Commit your changes. \
  ONLY WORK ON A SINGLE TASK. \
  If the PRD is complete, output <promise>COMPLETE</promise>.")

  echo "$result"

  if [[ "$result" == *"<promise>COMPLETE</promise>"* ]]; then
    echo "PRD complete after $i iterations."
    exit 0
  fi
done