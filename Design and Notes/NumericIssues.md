# Gravity Toy - Numeric Issues

## Big Takeaways
* For a game or a visualization, it would be much better to use positions at time _t_ on pre-defined idealized orbits than a live incremental gravity calculation. This would be guaranteed stable and also computationally much cheaper.
   * A sophisticated toy would switch from the computed location approach to a full gravity simulation only when necessary (e.g. when a significant mass body or artificial force is added to the scenario).

## Issues
* The scenarios with perfectly symmetrical starting positions and forces eventually break out of symmetrical motion. See issue #2 (Closed).
* The orbital simulations can have orbital traces that are partially polygonal and partially curved. See issue #10 (Closed).
* In the orbital simulations the orbits of bodies gradually grow / the bodies slow down (issue #37, Closed).

## Causes and Fixes
* As I began investigating the numeric issues, I discovered an underlying issue that magnified many of the undesired behaviors: I was originally using the Windows.Foundation.Point class for the simulation points because I saw the constructor signature was Point(double, double) and assumed X and Y were doubles internally. It turns out they are currently floats (see FoundationPointIssue.sln). Using floats originally caused some of the issues to be magnified and some to happen sooner or more frequently than they do with doubles.
   * This is spelled out in issue #36 and fixed in the linked commit.
* The symmetrical scenarios that break out of symmetry were caused by (a + b + a) not being equal to (a + a + b) for certain values of a and b with similar magnitudes. These one bit roundoff errors get magnified over many calculation cycles (and new errors are introduced) leading to eventual macro breakdown of symmetry.  See FloatingPointNotAssociative.cs in NumericTypesTests.sln for the specific trigger I found in the 5 bodies scenario. Also see the descriptions of Roundoff Error and Stability on pages 10-11 of _Numerical Recipes, 3e_.
   * This is fixed by rounding off (to achieve masking) the repeating calculations so that the errors in the lowest significant bits of the mantissa don't manifest in the calculations. See issue #2 (Closed).
* Issue #10 was being caused by adding very small values to large values (e.g. adding small fractions of a millimeter to 42000 km) and losing almost all precision.
   * Created addition precision check in the numeric investigations branch and then merged into master. This can be turned on or off per scenario. It doesn't prevent this issue, but displays debugging output when precision is being lost (which isn't visible until it's severe). Careful selection of calculation parameters can prevent or minimize it happening (too many iterations makes small delta values even smaller).
* Issue #37 is a case of cumulative inaccuracy in the simulation algorithm. I'm using linear interpolation per time step and of course orbits are continuous curves rather than polygons. I put the ISS orbit scenario into NumericTypesTests.sln and tested it with a wide range of values for the time step in the simulation (1 step/minute to 3,000,000 steps/minute). At 1 step/minute the simulation is using a 90-ish sided polygon for the first ISS orbit, which is of course wildly inaccurate. As the steps are made finer, the linear approximation becomes a closer and closer to the actual curve. At 3,000,000 steps per minute (= 50,000 steps per second) the accuracy is very good, with an error in ISS position of less than one meter per orbit.
   * A precisely accurate simulation using the current algorithm is computationally very expensive. It would be much better to use a more advanced approximation (or numerical solution). I'm assuming that even an expensive advanced algorithm that gives high accuracy is going to be much cheaper than achieving the same accuracy with brute force using the current algorithm.
   * The error looks like it is directly proportional to the number of steps per time (e.g. twice the steps -> 1/2 the error). This may indicate that there is a small tweak (or calcuable compensating factor) that would improve the accuracy of the current approach. See issue #45 for the next step in investigating this.


## Other Potential Fixes or Improvements to Investigate
* Understand the mathematical basis for the circular orbit error in the current algorithm and determine if it is correctable. (see issue #45)
* Polynomial instead of linear interpolation per step (see chapter 3 of _Numerical Recipes, 3e_).
* What do real open source astrometry simulations do? (see issue #31).
* ~~Higher precision math.~~
* Integral solution over a time interval for orbiting bodies (2-body calculation).
* Re-thinking and re-engineering of acceleration limits when objects "pass through" one another.
