<script>
  export let title = "No Title";  
</script>

<span>{title}</span>
<ul>
  <slot />
</ul>

<style>

  span {
    opacity: .5;
    
  }
  
  ul {
    gap: .5em;
    display: flex;
    flex-direction: column;
    list-style: none;
    padding-block: .5em;
    overflow: auto;
  }
</style>
