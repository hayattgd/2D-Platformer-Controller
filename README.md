# 2D-Platformer-Controller

2D Platformer Controller/Character movement for Unity.

**This repository is distributed under [MIT-LICENSE](https://github.com/hayattgd/2D-Platformer-Controller/blob/main/LICENSE).**

## Setup

Just put CharacterMovement.cs to your character and Create a empty game object in child and place it in foot and set it in Grouond Pos.

(Empty gameobject must placed in very Bottom of character. its used for detecting ground.)

### Tags

Add a tags that should be detected as ground. **(Leaving this default is not recommended!)**

For example:
Tilemap = "Ground" Character = "Player"
and Adding a "Ground" tag to List.

### Camera

if you want to let CharacterMovement handle camera move, just put Camera transform to Cam.

you can adjust smoothness and offset of camera.

you can of course empty Cam to handle camera movement yourself.

### Animation

Put a Animator component to Animator.

and, type a name of parameter in animator to receive X/YSpeed and IsGround.

### Behaviour

You can choose the script will handle input by self or your own script will handle it.

also, you can choose flip method. if you're not using SpriteRenderer, use Scaling. it will make X of Scale to -1 or 1 to flip.

## Customize

if you want to handle control your self, disable "Move By Self" and create a script that calls Control() in Update()

if you dont need it, just toggle on "Move By Self" and script will let you control character with Horizontal and Jump Axis.

(By default, horizontal set to left / right arrow key and A/D key and jump is set to space key.)

### Events

You can add your own script's function to customize character's behaviour.
