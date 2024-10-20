# Building Guide

This documentation will cover the building process of IceCraft.

## Building installable (prepped) archive

The `scripts/build-prepped` script is used to build IceCraft to create an installable build.

An installable build comes with:

- An IceCraft instance already having IceCraft bundled and registered into
  package database
- An installation script to install that instance to user `HOME`

Navigate to repository root and run `scripts/build-prepped`. The installable build should be in `bin/prep`.
