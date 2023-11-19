# Piper for Unity
Enables offline text to speach inside unity app

# Installation
- Add git repo as package Window -> Package Manager -> Add from Git URL https://github.com/mrtrizer/UnityPiper.git
- Download espeak-ng-data (You need only espeak-ng\share\espeak-ng-data from archive) and put into StreamingAssets - https://github.com/rhasspy/espeak-ng/releases/tag/2023.9.7-4
- Download trained voice model (both .onnx and .json files!) and put into StreamingAssets - https://huggingface.co/rhasspy/piper-voices/tree/v1.0.0
- Put Test.prefab from package on scene, enter paths to models in example component fields and click "Play"