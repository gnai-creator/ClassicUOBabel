#!/bin/bash

set -e

# Paths and project details
bootstrap_project="../src/ClassicUO.Bootstrap/src/ClassicUO.Bootstrap.csproj"
client_project="../src/ClassicUO.Client"
output_directory="../bin/dist"

# Target platform for Linux
target="linux-x64"

dotnet publish "$bootstrap_project" -c Release -o "$output_directory"
dotnet publish "$client_project" -c Release -p:NativeLib=Shared -p:OutputType=Library -r "$target" -o "$output_directory"

