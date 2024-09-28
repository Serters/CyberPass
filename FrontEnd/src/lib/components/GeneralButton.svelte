<script>
  import { FolderClosedIcon } from "lucide-svelte";
  import CategoryButton from "./CategoryButton.svelte";
  export let icon = FolderClosedIcon;
  export let label = "No Label";
  export let subLabel = null;
  export let active = false;
  export let removeAction = null;
</script>

<CategoryButton {active} on:click>
  <svelte:component this={icon} size="1em" opacity="50%" />
  <div class="wrapper">
    <span>
      {label}
    </span>
    {#if subLabel}
      <span class="secondary">
        {subLabel}
      </span>
    {/if}
  </div>
</CategoryButton>

<style>
  span {
    white-space: nowrap;
  }

  div {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
  }

  .secondary {
    color: var(--clr-text-secondary);
  }
</style>
