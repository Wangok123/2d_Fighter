# Testing and Migration Checklist

This refactoring is complete and ready for testing in Unity. Follow this checklist to ensure a smooth migration.

## ✅ Pre-Migration Verification

Before making any changes in Unity, verify these files are present:

- [ ] `MovementInputSystem.cs` - New movement system
- [ ] `AbilityInputSystem.cs` - New ability system  
- [ ] `MovementSystem.Legacy.cs` - Old system (deprecated)
- [ ] `ModularAbilitySystem.Legacy.cs` - Old system (deprecated)
- [ ] `Character.qtn` - Updated component definition
- [ ] `ModularCharacterConfig.cs` - Updated with command input settings
- [ ] `CommandInputSystem.cs` - Updated to use ModularConfig

## 📋 Migration Steps in Unity

### Step 1: Backup
- [ ] Create a backup of your project
- [ ] Commit all current work to version control
- [ ] Note: You can switch back to legacy systems if needed

### Step 2: Generate Quantum Code
- [ ] Open Unity Editor
- [ ] Go to Quantum menu → Generate Asset Code
- [ ] Wait for code generation to complete
- [ ] Check for any compilation errors

### Step 3: Update System Configuration
- [ ] Locate your Quantum system configuration (usually in SystemsConfig)
- [ ] Disable or remove: `MovementSystem` (use legacy version if needed)
- [ ] Disable or remove: `ModularAbilitySystem` (use legacy version if needed)
- [ ] Enable: `MovementInputSystem`
- [ ] Enable: `AbilityInputSystem`
- [ ] Ensure: `CommandInputSystem` is enabled

### Step 4: Update Entity Prototypes
For each character entity prototype:
- [ ] Open the prototype asset
- [ ] Find the `AttackData` component
- [ ] Remove any reference to `AttackConfig` (should already be removed)
- [ ] Verify `ModularConfig` reference is set

### Step 5: Update ModularCharacterConfig Assets
For each ModularCharacterConfig asset:
- [ ] Open the asset
- [ ] Set `CommandInputWindow` (recommended: 0.5)
- [ ] Set `MaxInputBufferSize` (recommended: 8)
- [ ] Verify all ability arrays are properly configured
- [ ] Verify ability unlocks are configured if using progression

### Step 6: Test Character Movement
For each character:
- [ ] Walk left/right
- [ ] Jump
- [ ] Double jump (if unlocked)
- [ ] Dash (if unlocked)
- [ ] Verify facing direction updates correctly
- [ ] Verify unlock system works at different levels

### Step 7: Test Character Abilities
For each character:
- [ ] Light attack
- [ ] Heavy attack  
- [ ] Heavy attack charging (if applicable)
- [ ] Combo system
- [ ] Special moves with input sequences
- [ ] Verify cooldowns work correctly
- [ ] Verify damage calculations are correct

### Step 8: Test Edge Cases
- [ ] Dead character (should not move or attack)
- [ ] Multiple characters simultaneously
- [ ] Abilities that aren't unlocked yet
- [ ] Character level changes
- [ ] Network synchronization (if applicable)

### Step 9: Performance Verification
- [ ] Profile the game in Unity Profiler
- [ ] Check system execution times
- [ ] Compare with legacy systems if possible
- [ ] Verify frame rate is stable

### Step 10: Clean Up (Optional)
After confirming everything works:
- [ ] Consider removing `.Legacy.cs` files if no longer needed
- [ ] Update any custom scripts that reference old systems
- [ ] Update team documentation

## 🐛 Troubleshooting

### Issue: Compilation Errors
**Solution:**
1. Check that Quantum code generation completed successfully
2. Verify all `using` statements are present
3. Check that `ModularCharacterConfig` has all required fields

### Issue: Characters Don't Move
**Solution:**
1. Verify `MovementInputSystem` is enabled in systems config
2. Check that entity has all required components (Transform2D, KCC2D, PlayerLink, MovementData, AttackData)
3. Verify `ModularConfig` reference is set on AttackData component

### Issue: Abilities Don't Execute
**Solution:**
1. Verify `AbilityInputSystem` is enabled in systems config
2. Check that `ModularConfig` has ability arrays configured
3. Verify abilities are unlocked (check UnlockedByDefault or RequiredLevel)
4. Check that CommandInputSystem is running for special moves

### Issue: Null Reference Errors
**Solution:**
1. Ensure all ModularCharacterConfig assets have required fields filled
2. Verify CommandInputWindow and MaxInputBufferSize are set
3. Check that entity prototypes have ModularConfig reference

### Issue: Unlock System Not Working
**Solution:**
1. Verify `AbilityUnlocks` array is configured in ModularCharacterConfig
2. Check that ability IDs match in unlocks and ability components
3. Verify CharacterLevel component is present and updating

## 📊 Performance Comparison (Expected)

After migration, you should see:
- **Reduced CPU usage**: Smaller systems with focused filters
- **Better frame times**: Less iteration overhead
- **Cleaner profiler**: Easier to identify bottlenecks
- **Faster configuration changes**: Single config source

## 🚀 Future Enhancements

When ready for further optimization:

1. **Signal-Based Systems**:
   - Add signal definitions to Signals.qtn
   - Generate code in Unity
   - Convert systems to SystemSignalsOnly
   - Systems only run when events occur

2. **Additional System Splitting**:
   - Separate defense abilities into DefenseAbilitySystem
   - Create MovementUnlockSystem for unlock validation
   - Split attack and special abilities into separate systems

3. **Data-Driven Abilities**:
   - Add more ability types
   - Create ability builder tools
   - Implement ability composition patterns

## 📞 Support

If you encounter issues:
1. Check the documentation files:
   - `REFACTORING_NOTES.md` - Technical details
   - `ARCHITECTURE_CHANGES.md` - Architecture explanation
   - `架构重构总结.md` - Chinese summary

2. Review the legacy systems for reference
   - `MovementSystem.Legacy.cs`
   - `ModularAbilitySystem.Legacy.cs`

3. You can temporarily switch back to legacy systems if needed

## ✨ Success Criteria

Migration is successful when:
- ✅ All characters move correctly
- ✅ All abilities execute properly
- ✅ Unlock system works as expected
- ✅ No compilation errors
- ✅ No runtime errors in console
- ✅ Network synchronization works (if applicable)
- ✅ Performance is same or better than before

---

**Note**: This refactoring maintains the same functionality as before, just with better code organization and performance. The gameplay should be identical to the legacy systems.
