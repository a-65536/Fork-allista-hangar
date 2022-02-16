#!/bin/bash

cd $(dirname "$0")

make_mod_release  -s 'Source' \
-e '*/ForModders/*' '*/config.xml' '*.user' '*.orig' '*.mdb' '*.pdb' \
'*/System.*.dll' '*/Mono.*.dll' '*/Unity*.dll' \
'GameData/000_AT_Utils/Plugins/AnimatedConverters.dll' \
'GameData/000_AT_Utils/Plugins/SubmodelResizer.dll' \
'GameData/ConfigurableContainers/Parts/*' \
'GameData/000_AT_Utils/ResourceHack.cfg' \
-i '../AT_Utils/GameData' '../AT_Utils/ConfigurableContainers/GameData'
