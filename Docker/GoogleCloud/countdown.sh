#!/bin/bash

echo 'Starting countdown...'
count=40
until [ ]; do
  sleep 0.25
  count=$((count - 1))
  echo "Countdown counter => $count"
done
