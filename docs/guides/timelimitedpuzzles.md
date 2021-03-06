---
layout: index
title: Timelimitedpuzzles
---

*This tutorial was published by Alex in his blog [1](http://www.textadventures.co.uk/blog/2012/02/27/time-limited-puzzles)*

When I was at Perins School last week, I was asked about puzzles with a time limit. For example, the player opens a cupboard, inside which is a hungry alien. How do you give the player 10 seconds to kill the alien, before the alien kills them instead?

This is pretty straightforward to handle, because in Quest you can run scripts after a certain number of seconds. Here’s a quick how-to:

First, add the cupboard and alien objects. The alien should be inside the cupboard. For the cupboard, go to the Container tab. Choose “Container” from the type list, and untick the “Is open” box so that the cupboard is closed when the game begins.

<image:tut_timelimit1.png>

Now we want to run a script when the player opens the object. We’ll tell the player they’ve surprised the sleeping (and hungry) alien, then give them 10 seconds to get rid of the alien before it kills them. To do this, scroll down to “After opening the object”, and add a “Print a message” script. Next, add another script – from the Timers section, choose “Run a script after a number of seconds”.

<image:tut_timelimit2.png>

You can now specify how many seconds to wait before something else happens. In this case, 10 seconds. After 10 seconds, we want to see if the “alien” object is still visible. If so, print a message and kill the player. If not, we don’t need to do anything.

So, all we need to do is add an “If” inside the “After 10 seconds” script, as shown below:

<image:tut_timelimit3.png>

Finally, we just need to implement a way to solve the puzzle. Let’s add a flame thrower object. When the player uses the flame thrower on the alien, the alien bursts into flames.

Add an object called “flame thrower”, then on the “Use/Give” tab scroll down to “Use this on (other object)”. Select “Handle objects individually”, add “alien”, and then edit the script. Add a “print a message” command to say something to the player, then add a “Remove object” command to remove the alien from play.

The resulting script looks like this:

<image:tut_timelimit4.png>
