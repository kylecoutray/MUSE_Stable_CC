"""
generate_trials_interactive.py

Prompts the user for:
  • A folder of stimulus files
  • times_as_target
  • distractor_ratio
  • trial timing parameters
  • number_of_blocks

Then:
 1) Builds one StimIndex→Filename mapping file (tab-delimited).
 2) Creates T = (num_stimuli × times_as_target) total trials.
 3) Splits these T trials evenly into B blocks, where B = number_of_blocks.
 4) Writes B separate WorkingMemory_TrialDef_array<block>.txt files, each containing its block’s trials.

Usage:
  python generate_trials__with_blocks_interactive.py
"""

import os
import random
import math
import argparse

def list_stim_files(stim_dir):
    """Return a sorted list of all non-hidden, non-.meta files in stim_dir."""
    filenames = []
    for fname in os.listdir(stim_dir):
        if fname.startswith("."):
            continue
        if fname.lower().endswith(".meta"):
            continue
        full = os.path.join(stim_dir, fname)
        if os.path.isfile(full):
            filenames.append(fname)
    filenames.sort()
    return filenames

def write_stim_map(filenames, output_path):
    """Write tab-delimited StimIndex→FileName (index starts at 0, without extension)."""
    with open(output_path, "w") as f:
        f.write("StimIndex\tFileName\n")
        for idx, fn in enumerate(filenames):
            basename = os.path.splitext(fn)[0]
            f.write(f"{idx}\t{basename}\n")

def build_trial_pools(num_stim, times_as_target, distractor_ratio):
    """
    1. T = num_stim * times_as_target
    2. D = round(T * distractor_ratio)
    3. times_as_distractor = round(3 * times_as_target * distractor_ratio)
       (Ensures 3 slots per distractor trial.)
    4. TargetPool: each stim index repeated times_as_target (length=T), shuffled.
    5. DistractorPool: each stim index repeated times_as_distractor (length=num_stim*times_as_distractor), shuffled.
    """
    T = num_stim * times_as_target
    D = int(round(T * distractor_ratio))
    times_as_distractor_float = 3 * times_as_target * distractor_ratio
    times_as_distractor = int(round(times_as_distractor_float))

    # Ensure total distractor roles = 3 * D
    total_distractor_roles = num_stim * times_as_distractor
    if total_distractor_roles != 3 * D:
        print(f"Warning: computed times_as_distractor = {times_as_distractor},")
        print(f"         but num_stim × times_as_distractor = {total_distractor_roles},")
        print(f"         which ≠ 3 × D ({3*D}).")
        print("         Adjusting times_as_distractor to make num_stim × times_as_distractor = 3 × D.")
        times_as_distractor = (3 * D) // num_stim
        print(f"         → times_as_distractor is now {times_as_distractor}.\n")

    # Build & shuffle TargetPool
    TargetPool = []
    for stim in range(num_stim):
        TargetPool += [stim] * times_as_target
    random.shuffle(TargetPool)

    # Build & shuffle DistractorPool
    DistractorPool = []
    for stim in range(num_stim):
        DistractorPool += [stim] * times_as_distractor
    random.shuffle(DistractorPool)

    return T, D, times_as_distractor, TargetPool, DistractorPool

def assign_distractors_to_trials(TargetPool, DistractorPool, num_distractor_trials):
    """
    Mark exactly D trials as distractor trials, then assign 3 distractors/trial
    from DistractorPool, ensuring:
      • No distractor == target
      • No duplicate distractor within the same trial
      • Every slot in DistractorPool is used exactly once
    Returns:
      is_distr (length=T list of True/False),
      DistractorsPerTrial (length=T list where each is [] or [d0,d1,d2]).
    """
    T = len(TargetPool)
    # 1) Mark D trials as distractor trials
    is_distr = [False] * T
    for i in range(num_distractor_trials):
        is_distr[i] = True
    random.shuffle(is_distr)

    # 2) Flatten DistractorPool to slots
    total_slots = len(DistractorPool)
    if total_slots != 3 * num_distractor_trials:
        raise ValueError(
            f"Total distractor slots ({total_slots}) != 3 * D ({3 * num_distractor_trials})."
        )
    DistractorSlots = DistractorPool.copy()

    # 3) Assign slots
    DistractorsPerTrial = [[] for _ in range(T)]
    slot_idx = 0
    num_stim = len(set(DistractorPool))

    for trial_idx, targ in enumerate(TargetPool):
        if not is_distr[trial_idx]:
            DistractorsPerTrial[trial_idx] = []
            continue

        assigned = []
        for _ in range(3):  # pick 3 slots
            attempts = 0
            while True:
                if slot_idx >= total_slots:
                    # No more slots: fallback to random valid stimulus
                    valid_choices = [
                        idx for idx in range(num_stim)
                        if idx != targ and (idx not in assigned)
                    ]
                    fallback = random.choice(valid_choices) if valid_choices else ((targ + 1) % num_stim)
                    assigned.append(fallback)
                    break

                candidate = DistractorSlots[slot_idx]
                if candidate != targ and (candidate not in assigned):
                    assigned.append(candidate)
                    slot_idx += 1
                    break
                else:
                    # Try to swap with a valid later slot
                    swap_idx = None
                    for j in range(slot_idx + 1, total_slots):
                        if (DistractorSlots[j] != targ) and (DistractorSlots[j] not in assigned):
                            swap_idx = j
                            break
                    if swap_idx is None:
                        # Fallback if no valid swap
                        valid_choices = [
                            idx for idx in range(num_stim)
                            if idx != targ and (idx not in assigned)
                        ]
                        fallback = random.choice(valid_choices) if valid_choices else ((targ + 1) % num_stim)
                        assigned.append(fallback)
                        break
                    else:
                        DistractorSlots[slot_idx], DistractorSlots[swap_idx] = (
                            DistractorSlots[swap_idx],
                            DistractorSlots[slot_idx],
                        )
                attempts += 1
                if attempts > 10000:
                    valid_choices = [
                        idx for idx in range(num_stim)
                        if idx != targ and (idx not in assigned)
                    ]
                    fallback = valid_choices[0] if valid_choices else ((targ + 1) % num_stim)
                    assigned.append(fallback)
                    break

        DistractorsPerTrial[trial_idx] = assigned

    return is_distr, DistractorsPerTrial

def pick_positions(num_positions, used_positions=set()):
    """
    Return num_positions distinct (x,y,0) from:
      x in [-4..4], y in [-2..2], excluding any in used_positions.
    """
    all_grid = [(x, y, 0) for x in range(-4, 5) for y in range(-2, 3)]
    available = [pt for pt in all_grid if pt not in used_positions]
    if len(available) < num_positions:
        raise ValueError("Not enough free coordinates to place all stimuli.")
    return random.sample(available, num_positions)

def split_into_blocks(trials_data, num_blocks):
    """
    Given a list `trials_data` of length T, split it into `num_blocks` contiguous chunks
    as evenly as possible, returning a list of lists [[block1_trials], [block2_trials], ...].
    """
    T = len(trials_data)
    base_size = T // num_blocks
    remainder = T % num_blocks

    blocks = []
    start = 0
    for b in range(num_blocks):
        size = base_size + (1 if b < remainder else 0)
        end = start + size
        blocks.append(trials_data[start:end])
        start = end
    return blocks

def main():
    print("\n--- GENERATE WORKING MEMORY TRIAL FILES WITH BLOCKS (Interactive) ---\n")

    # 1) Stimulus directory
    while True:
        stim_dir = input("1) Enter the path to your stimulus folder: ").strip()
        if os.path.isdir(stim_dir):
            stim_files = list_stim_files(stim_dir)
            if len(stim_files) == 0:
                print("   › That folder has no stimulus files. Try again.")
                continue
            break
        else:
            print("   › Path not found or not a directory. Try again.")
    num_stim = len(stim_files)
    print(f"   → Found {num_stim} stimulus files.\n")

    # 2) times_as_target
    while True:
        try:
            times_as_target = int(input("2) How many times should each stimulus appear as TARGET? ").strip())
            if times_as_target < 1:
                print("   › Must be at least 1. Try again.")
                continue
            break
        except ValueError:
            print("   › Invalid integer. Try again.")

    # 3) distractor_ratio
    while True:
        try:
            distractor_ratio = float(input(
                "3) Fraction of total trials that should include 3 post-sample distractors (e.g. 0.5): "
            ).strip())
            if not (0.0 <= distractor_ratio <= 1.0):
                print("   › Enter a value between 0.0 and 1.0. Try again.")
                continue
            break
        except ValueError:
            print("   › Invalid float. Try again.")

    # 4) Compute T, D, times_as_distractor
    T, D, times_as_distractor, TargetPool, DistractorPool = build_trial_pools(
        num_stim, times_as_target, distractor_ratio
    )
    print(f"\n   → Total trials (T) = {T}")
    print(f"   → Distractor‐inclusive trials (D) = {D}")
    print(f"   → Computed times_as_distractor = {times_as_distractor}")
    print(f"     (Each of {num_stim} stimuli appears {times_as_distractor} times as a distractor.)\n")

    # 5) trial_id_prefix
    trial_id_prefix = input("4) Enter prefix for each TrialID (e.g. WMn1.TDSIM): ").strip()

    # 6) block_count ⇒ number_of_blocks
    while True:
        try:
            num_blocks = int(input("5) How many blocks do you want to split these trials into? ").strip())
            if num_blocks < 1 or num_blocks > T:
                print(f"   › Enter an integer between 1 and {T}. Try again.")
                continue
            break
        except ValueError:
            print("   › Invalid integer. Try again.")

    # 7) block_count (legacy name, but we’ll ignore since we ask for num_blocks)
    #    We’ll still prompt for it so the numbering matches your original script’s menu:
    block_count = 1  # unused except as column value per trial

    # 8) display_sample_duration
    while True:
        try:
            display_sample_duration = float(input("6) Enter DisplaySampleDuration (e.g. 0.5): ").strip())
            break
        except ValueError:
            print("   › Invalid float. Try again.")

    # 9) post_sample_delay_duration
    while True:
        try:
            post_sample_delay_duration = float(input("7) Enter PostSampleDelayDuration (e.g. 0.5): ").strip())
            break
        except ValueError:
            print("   › Invalid float. Try again.")

    # 10) display_post_sample_distractors_duration
    while True:
        try:
            display_post_sample_distractors_duration = float(input(
                "8) Enter DisplayPostSampleDistractorsDuration (e.g. 0.5): "
            ).strip())
            break
        except ValueError:
            print("   › Invalid float. Try again.")

    # 11) pre_target_delay_duration
    while True:
        try:
            pre_target_delay_duration = float(input("9) Enter PreTargetDelayDuration (e.g. 3.0): ").strip())
            break
        except ValueError:
            print("   › Invalid float. Try again.")

    # 12) Output filenames
    stim_map_base = input("\n10) Enter base name for stimulus map (e.g. WorkingMemory_StimDef_array): ").strip()
    out_stim_map = stim_map_base + ".txt"
    base_trial_def_name = input("11) Enter the base name for trial definition files (e.g. WorkingMemory_TrialDef_array): ").strip()

    # 13) Write stimulus mapping
    write_stim_map(stim_files, out_stim_map)
    print(f"\n   → Wrote stimulus mapping to: {out_stim_map}")

    # 14) Assign distractors
    is_distr_trials, DistractorsPerTrial = assign_distractors_to_trials(
        TargetPool, DistractorPool, D
    )

    # 15) Generate all trial rows first (in memory), then split by block
    header_cols = [
        "TrialID",
        "BlockCount",
        "DisplaySampleDuration",
        "PostSampleDelayDuration",
        "DisplayPostSampleDistractorsDuration",
        "PreTargetDelayDuration",
        "SampleStimLocation",
        "SearchStimIndices",
        "SearchStimLocations",
        "SearchStimTokenReward",
        "PostSampleDistractorStimIndices",
        "PostSampleDistractorStimLocations",
    ]

    all_trials = []  # Each entry is a single tab-delimited string (one trial)

    for trial_idx in range(T):
        trial_id = f"{trial_id_prefix}{trial_idx+1}"

        # Timing
        dsamp   = display_sample_duration
        ps_del  = post_sample_delay_duration
        dsp_distr = display_post_sample_distractors_duration

        if is_distr_trials[trial_idx]:
            pretd = pre_target_delay_duration - (ps_del + dsp_distr)
            if pretd < 0:
                raise ValueError(
                    f"PreTargetDelayDuration < 0 for trial {trial_idx}. Check inputs."
                )
        else:
            pretd = pre_target_delay_duration

        # Sample at center
        targ_idx   = TargetPool[trial_idx]
        sample_pos = (0, 0, 0)

        # Build arrays
        if is_distr_trials[trial_idx]:
            # 3 post-sample distractors
            d_indices = DistractorsPerTrial[trial_idx]   # length=3
            post_distr_indices = d_indices[:]
            post_distr_locs = [(-3, 0, 0), (0, 0, 0), (3, 0, 0)]

            # Search: pick any 2 of the 3 to accompany sample
            search_two = random.sample(d_indices, k=2)
            search_indices = [targ_idx] + search_two
            used = {sample_pos}
            search_locs = pick_positions(3, used)
            search_rewards = [2, -1, -1]

        else:
            # Solo → pick 2 random distractors for the search display
            possible = [i for i in range(len(stim_files)) if i != targ_idx]
            search_two = random.sample(possible, k=2)
            search_indices = [targ_idx] + search_two
            used = {sample_pos}
            search_locs = pick_positions(3, used)
            search_rewards = [2, -1, -1]
            post_distr_indices = []
            post_distr_locs = []

        # Format fields
        sample_loc_str = f"[{sample_pos[0]}, {sample_pos[1]}, {sample_pos[2]}]"
        search_inds_str = "[" + ",".join(str(i) for i in search_indices) + "]"
        search_locs_str = "[" + ",".join(f"[{p[0]}, {p[1]}, {p[2]}]" for p in search_locs) + "]"
        search_rew_str = "[" + ",".join(str(r) for r in search_rewards) + "]"
        post_inds_str = "[" + ",".join(str(i) for i in post_distr_indices) + "]"
        post_locs_str = "[" + ",".join(f"[{p[0]}, {p[1]}, {p[2]}]" for p in post_distr_locs) + "]"

        row_elems = [
            trial_id,
            str(block_count),  # this column is static (1) per trial
            f"{dsamp:.2f}",
            f"{ps_del:.2f}",
            f"{dsp_distr:.2f}",
            f"{pretd:.2f}",
            sample_loc_str,
            search_inds_str,
            search_locs_str,
            search_rew_str,
            post_inds_str,
            post_locs_str,
        ]
        all_trials.append("\t".join(row_elems))

    # 16) Split all_trials into blocks
    blocks = split_into_blocks(all_trials, num_blocks)

    # 17) Write each block’s trials to its own file
    for b, block_trials in enumerate(blocks, start=1):
        filename = f"{base_trial_def_name}{b}.txt"
        with open(filename, "w") as fout:
            fout.write("\t".join(header_cols) + "\n")
            for line in block_trials:
                fout.write(line + "\n")
        print(f"   → Wrote block {b} trials to: {filename}")

    print("\n--- ALL DONE ---\n")

if __name__ == "__main__":
    main()
