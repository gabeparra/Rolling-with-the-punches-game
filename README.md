# AiForGame

- **Unity Version:** 6000.3.7f1

1. Clone the repo and switch to your branch:
   ```
   git clone https://github.com/gabeparra/AiForGame.git
   cd AiForGame
   git checkout <your-branch>
   ```

2. Open the project in **Unity 6000.3.7f1**

3. Set the render pipeline:
   - Go to **Edit > Project Settings > Quality**
   - Set the **Render Pipeline Asset** to `PC_RPAsset` (found in `Assets/Settings/`)

4. Open the main scene: `Assets/Scenes/Scene 1.unity`

5. Press **Play**

## PolygonWestern Textures

If PolygonWestern assets appear pink/untextured:

1. Open **Window > Rendering > Render Pipeline Converter**
2. Select **Built-in to URP**
3. Check **Material Upgrade**
4. Click **Initialize Converters**, then **Convert Assets**

## Syncing Your Branch with Main

To get the latest working version into your branch:

```
git fetch origin
git merge origin/main -m "Merge latest main"
```

If you get merge conflicts and want to keep main's version:

```
git checkout --theirs .
git add .
git commit -m "Resolve conflicts, keep main version"
```

## Pushing Your Work

```
git add .
git commit -m "Your commit message"
git push origin <your-branch>
```

Then create a Pull Request on GitHub to merge into main.
