# Flocking
Flocking simulations and mini game

## Author
Minas Mina  
Contact: [minasm1990 [at] gmail.com](mailto:minasm1990@gmail.com)  

## About this project
_Flocking_ is a project that implements steering behaviors of flocks of birds. It has three modes:
* Wander Behavior
* Shape Formation
* "Gather the Flock" mini game

The implementation of the steering behaviors can be found in: 
```
\source\Assets\SteeringBehaviors
```
The behaviors can be used autonomously, even in non-GUI applications.

## Modes
### Wander Behavior
In this mode the user can see how the parameters for _cohesion_, _separation_ and _velocity match_ affect the formation of a flock that follows a leader (controlled by the user).

By pressing the _ESC_ key, the simulation pauses and you can see how you can control the leader bird, rotate the camera etc.

![Alt text](/screenshots/wander behavior.png?raw=true "Wander Behavior mode")

### Shape Formation
In this mode the user can lead a flock of birds that form a specific shape.

The birds of the flock tend to move towards their "predefined" positions around the center of the shape. They also avoid each other and other obstacles (e.g land).

The available models are:
* horse.txt
* whale.txt
* dragon.txt

![Alt text](/screenshots/shape formation.png?raw=true "Shape Formation mode")

### Gather the Flock mini game
This is a simple mini game in which the player must move close to birds to add them to his/her flock. The goal is to make a flock as large as possible in 60 seconds.  
Birds become part of the flock when their distance to the player is under a threshold.

![Alt text](/screenshots/gather the flock.png?raw=true "'Gather the Flock' mini game")