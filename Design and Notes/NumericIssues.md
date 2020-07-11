# Gravity Toy - Numeric Issues

## Big Takeaways
* For a game or a visualization, it would be much better to use positions at time _t_ on pre-defined idealized orbits than a live incremental gravity calculation. This would be guaranteed stable and also computationally cheaper.

## Issues
* The scenarios with perfectly symmetrical starting positions and forces eventually break out of symmetrical motion. See issue #2 (Closed).
* The orbital simulations can have orbital traces that are partially polygonal and partially curved. See issue #10 (Closed).
* In the orbital simulations the orbits of bodies gradually grow / the bodies slow down (issue #37).

## Causes and Fixes
* As I began investigating the numeric issues, I discovered this underlying issue: I was originally using the Windows.Foundation.Point class for the simulation points because I saw the constructor signature was Point(double, double) and assumed X and Y were doubles internally. It turns out they are currently floats (see FoundationPointIssue.sln). Using floats originally caused some of the issues to be magnified and some to happen sooner or more frequently than they do with doubles.
   * This is spelled out in issue #36 and fixed in the linked commit.
* The symmetrical scenarios that break out of symmetry were caused by (a + b + a) not being equal to (a + a + b) for certain values of a and b with similar magnitudes. These one bit roundoff errors get magnified over many calculation cycles (and new errors are introduced) leading to eventual macro breakdown of symmetry.  See FloatingPointNotAssociative.cs in NumericTypesTests.sln for the specific trigger I found in the 5 bodies scenario. Also see the descriptions of Roundoff Error and Stability on pages 10-11 of _Numerical Recipes, 3e_.
   * This is fixed by rounding off (to achieve masking) the repeating calculations so that the errors in the lowest significant bits of the mantissa don't manifest in the calculations. See issue #2 (Closed).
* Issue #10 was being caused by adding very small values to large values (e.g. adding small fractions of a millimeter to 42000 km) and losing almost all precision.
   * Created addition precision check in the numeric-investigations branch and then merged into master. This can be turned on or off per scenario. It doesn't prevent this issue, but displays debugging output when precision is being lost (which isn't visible until it's severe). Careful selection of calculation parameters can prevent or minimize it happening (too many iterations makes small delta values even smaller).
* Issue #37: cause needs investigation.


## Other Potential Fixes to Investigate
* Polynomial instead of linear interpolation per step (chapter 3 of _Numerical Recipes, 3e_).
* What do real open source astrometry simulations do? (see issue #31).
* Higher precision math.
* Integral solution over a time interval for orbiting bodies (2-body calculation).
* Re-thinking and re-engineering of acceleration limits when objects "pass through" one another.
