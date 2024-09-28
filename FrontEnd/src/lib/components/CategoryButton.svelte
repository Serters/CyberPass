<script>
  export let active = false;
</script>

  <!--<svelte:component this={icon} /> -->
  <button on:click class:selected={active}>
    <slot />
  </button>

<style>
  button {
    width: 100%;
    display: flex; 
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem;
    --_clr-btn: none;
    background-color: var(--_clr-btn);
    color: inherit;
    border: none;
    cursor: pointer;
    transition: 0.3s;
    border-radius: 0.3em;
    &:hover {
      --_clr-btn: color-mix(in oklch, var(--clr-accent) 30%, transparent);
    

    }
    &.selected {
      --_clr-btn: var(--clr-accent);
    }
  }
</style>
