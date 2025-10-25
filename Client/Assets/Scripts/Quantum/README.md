# Quantum Scripts Folder Structure

This folder contains organized Quantum engine related scripts for the 2D Fighter project.

## Folder Structure

- **Qtn/**: Contains Quantum QTN (Quantum Type Notation) related scripts
- **System/**: Contains Quantum System scripts (game systems that run on the Quantum simulation)
- **Simulation/**: Contains Quantum Simulation scripts and the Quantum.Simulation assembly reference

## Usage

Place your Quantum-related scripts in the appropriate subfolder based on their purpose:
- QTN definitions and types go in `Qtn/`
- Game systems implementations go in `System/`
- Simulation-specific code goes in `Simulation/`

The `Quantum.Simulation.asmref` file in the Simulation folder references the Quantum.Simulation assembly definition, allowing scripts in this folder to access Quantum simulation APIs.
