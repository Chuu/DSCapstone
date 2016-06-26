#!/bin/bash

unzip ./Coursera-SwiftKey.zip
find ./final/en_US/*.txt -exec cat {} \; >> ./final/en_US/all.txt
rm ./Coursera-SwiftKey.zip

