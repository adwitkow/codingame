Rank:
* **3**/47 globally
* **2**/9 in Poland

## The Goal
Protect your base from incoming attacks and outlive your opponent.
## Rules
Both players controls a team of 3 **heroes**. The teams start out at opposite corners of the map, near their **base**. Throughout the game **monsters** will appear regularly on the edges of the map. If a **monster** reaches your base, it will deal **damage**. If your base takes too much damage, you lose. Thankfully, your **heroes** can kill **monsters**.
### The map
The game is played on a map 17630 units wide by 9000 units high. The coordinate X=0, Y=0 is the top left pixel. Fog makes it impossible to know the positions of all **monsters** and rival **heroes**. You need to have them within 2200 units from one of your **heroes** or 6000 from your **base**. Each **base** can take a maximum of 3 points of damage before being destroyed. Multiple **entities** (heroes & monsters) can occupy the same coordinates, there is no collision.
### Heroes
Every turn, you must provide a command for each **hero**. They may perform any of the following commands:
* **WAIT**, the hero remains where he is.
* **MOVE**, followed by map coordinates will make the hero advance towards that point by a maximum of 800 units.
* **SPELL**, followed by a spell action, as detailed in the Spells section further below.

**Heroes** cannot be killed and cannot leave the map. After a **hero**'s move phase, any **monsters** within 800 units will suffer 2 points of damage, even when they have no health left.
### Monsters
Every **monster** appears with a given amount of **health**. If at the end of a turn, a monster's **health** has dropped to zero or below, the **monster** is removed from the game. **Monsters** will appear randomly, but symmetrically from the map edges outside of the player's bases. They appear with a random moving direction. **Monsters** will always advance in a straight line at a speed of 400 units per turn. If a **monster** comes within 5000 units of a **base** at the end of a turn, it will **target** that base. When **targeting** a base, a monster will move **directly towards** that **base** and can no longer leave the map. If a **monster** is **pushed** (with a WIND command) outside the radius of a **targeted** base, it will stop targeting and start moving in a randomly selected direction. If a **monster** comes within 300 units of a **base** at the end of a turn, as long as it has not been killed on this turn, it will disappear and deal the **base** 1 point of damage. Each subsequent **monster** may have slightly more starting health than any previous **monster**.
### Spells
Your team will also acquire 1 point of **mana** per damage dealt to a monster. Mana is shared across the team and can be use by heroes to cast various **spells**. There is no upper mana limit. A spell command may have **parameters**, which you must separate with a space. command parameters mana cost effect range

| command | parameters         | mana cost | effect | range |
| ------- |:------------------:| ---------:|-------:|------:|
| WIND    | <x> <y>            | 10        | All entities (except your own heroes) within 1280 units are **pushed** 2200 units in the direction from the spellcaster to x,y | 1280 |
| SHIELD  | <entityId>         | 10        | The target entity cannot be targeted by spells for the next 12 rounds. | 2200 |
| CONTROL | <entityId> <x> <y> | 10        | Override the target's next action with a step towards the given coordinates. | 2200 |

A hero may only cast a spell on entities that are within the spell's range from the hero.
## WIND Example
A hero uses WIND at position (6000, 6000) towards (6000, 5000).
![](https://www.codingame.com/servlet/fileservlet?id=20669980430629)
There are 2 monsters within 1280 units around the hero.
![](https://www.codingame.com/servlet/fileservlet?id=20669992024930)
WIND 6000 5000 The vector (0,-1) describes the direction between the hero and the target point. ![](https://www.codingame.com/servlet/fileservlet?id=20669974783504)
The monsters all move 2200 in the direction defined by the vector.
## Victory Conditions
* Outlive your opponent for **220 turns**.
* Have more base health points than your opponent after **220 turns**. 
* In case of a tie, have gained the highest amount of **mana** outside the radius of your **base**.
## Defeat Conditions
* Your program does not provide a valid command in time.
* Your base's health reaches zero.
## Technical Details
* After an entity moves towards a point, its coordinates are **truncated** (when below halfway across the map) or **rounded up** (when above halfway across the map), only then are distance-based calculations performed (such as monster damage).
* Each hero and monster has a unique id.
* Spells are cast in the order of the received output. This means a spell may be **cancelled** if another hero spent the necessary mana earlier in the turn. 
* If an entity is being moved via a CONTROL from multiple sources at once, it will move to the average of all computed destinations. 
* If an entity is being moved via a WIND from multiple sources at once, it will move to the sum of all given directions. 
* SHIELD also protects entities from receiving a new SHIELD. 
* Using a spell against a shielded entity still costs mana. 
* Players are not given the coordinates of monsters outside the map. 
* In case of a tie, the player who gained the highest amount of **mana** outside the radius of their **base** will win.
## Action order for one turn
1. Wait for **both** players to output 3 commands.
2. CONTROL spells are applied to the targets and will only be effective on the next turn, after the next batch of commands. 
3. SHIELD spells are applied to the targets and will only be effective on the next turn, after the next batch of commands. Does **not** protect from a CONTROL or WIND from the same turn.
4. MOVE all heroes.
5. Heroes attack monsters in range and produce mana for each hit.
6. WIND spells are applied to entities in range.
7. MOVE all monsters according to their current speed, unless they were **pushed** by a wind on this turn.
8. SHIELD countdowns are decremented.
9. New monsters appear. Dead monsters are removed
### 🐞 Debugging tips
* Hover over entities to see information about them
* Append text after any command and that text will appear above the hero 
* Press the gear icon on the viewer to access extra display options 
* Use the keyboard to control the action: space to play/pause, arrows to step 1 frame at a time
## Game Input
##### Initialization Input
**Line 1**: two integers baseX and baseY for the coordinates of your base. The enemy base will be at the opposite side of the map.
**Line 2**: one integer heroesPerPlayer is always 3. 
##### Input for One Game Turn
**First 2 lines**: two integers baseHealth and mana for the remaining health and mana for both players. Your data is always given first.
**Next line**: entityCount for the amount of game entities currently visible to you.
**Next entityCount lines**: 11 integers to describe each entity:
* id: entity's unique id.
* type:
  * 0: a monster
  * 1: one of your heroes 
  * 2: one of your opponent's heroes 
* x & y: the entity's position. 
* shieldLife: the number of rounds left until entity's shield is no longer active. 0 when no shield is active.
* isControlled: 1 if this entity is under a CONTROL spell, 0 otherwise. The next 5 integers only apply to monsters (will be -1 for heroes). 
* health: monster's remaining health points. 
* vx & vy: monster's current speed vector, they will add this to their position for their next movement. 
* nearBase: 1: if monster is **targeting** a base, 0 otherwise. 
* threatFor: 
  * With the monster's current trajectory — if nearBase is 0:
    * 0: it will never reach a base. 
    * 1: it will eventually reach your base. 
    * 2: it will eventually reach your opponent's base.
  * If nearBase is 1:
    * 1 if this monster is **targeting** your base, 2 otherwise.
##### Output
Three lines, one for each hero, containing one of the following actions:
* WAIT: the hero does nothing.
* MOVE followed by two integers (x,y): the hero moves 800 towards the given point. 
* SPELL followed by a spell command: the hero attempts to cast the given spell.

You may append text to a command to have it displayed in the viewer above your hero. Examples: 
* MOVE 8000 4500
* SPELL WIND 80 40 casting a wind spell! 
* SPELL SHIELD 1
* WAIT nothing to do...

You must provide a valid command to all heroes each turn, even if they are being controlled by your opponent.
## Constraints
* Response time per turn ≤ 50ms
* Response time for the first turn ≤ 1000ms 