<div id="app">
    <slot />
</div>

<style>
  #app {
    display: grid;
    grid-template-columns: 2.5fr 5fr 5fr;
    position: relative;
    flex: 1;
    gap: 2px;
  }
</style>