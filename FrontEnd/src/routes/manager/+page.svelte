<script lang="ts">
  import { onMount } from "svelte";
  import {
    PlusIcon,
    WandSparkles,
    KeyRound,
    LockKeyhole,
    Trash,
    DiscIcon,
  } from "lucide-svelte";
  import Category from "../../lib/components/Category.svelte";
  import CategoryButton from "../../lib/components/CategoryButton.svelte";

  // @ts-ignore
  import { PUBLIC_SWAG_TOKEN, PUBLIC_API_URL } from "$env/static/public";
  import GeneralButton from "../../lib/components/GeneralButton.svelte";
  import { fade, fly, slide } from "svelte/transition";
  import { flip } from "svelte/animate";

  let folders = [];

  let entries = [];


  let searchString = "";

  let formItems = {
    username: "",
    password: "",
    website: "",
    note: "",
  };

  const fetchFolders = async () => {
    const response = await fetch(`${PUBLIC_API_URL}/api/Folder`, {
      method: "GET",
      headers: {
        accept: "text/plain",
        Authorization: `Bearer ${PUBLIC_SWAG_TOKEN}`, // Replace with actual JWT token
      },
    });

    if (response.ok) {
      const data = await response.json();
      console.log(data);
      // Map the API response and include your default "Other" folder with the icon
      folders = data.map(({ name, id }) => ({ label: name, id }));
    } else {
      console.error("Failed to fetch folders:", response.status);
    }
  };

  let folderId = -2;

  const refreshEntries = async () => {
    const folder = folderId > -1 ? `Folder/${folderId}` : "";

    const response = await fetch(
      `${PUBLIC_API_URL}/api/PasswordEntry/${folder}`,
      {
        method: "GET",
        headers: {
          accept: "text/plain",
          Authorization: `Bearer ${PUBLIC_SWAG_TOKEN}`, // Replace with actual JWT token
        },
      }
    );

    if (response.ok) {
      entries = await response.json();
      console.log(entries);
    } else {
      console.error("Failed to fetch folders:", response.status);
    }
  };

  const fetchFromId = async (fldId = -1) => {
    if (fldId === folderId) return;
    folderId = fldId;
    refreshEntries();
  };

  const allFolderView = (second) => {};

  const handleClick = (args) => {
    console.log(args);
  };

  const createPassword = async (formData) => {
    console.log(formData);
    const response = await fetch(`${PUBLIC_API_URL}/api/PasswordEntry`, {
      method: "POST",
      headers: {
        accept: "*/*",
        "Content-Type": "application/json",
        Authorization: `Bearer ${PUBLIC_SWAG_TOKEN}`, // Replace with actual JWT token
      },
      body: JSON.stringify(formData),
    });

    if (response.ok) {
      console.log("Successfully posted");
      refreshEntries();
    } else {
      console.error("Failed to post:", response.status);
    }
  };

  const updatePassword = async (id, formData) => {
    console.log(formData);
    const response = await fetch(`${PUBLIC_API_URL}/api/PasswordEntry/${id}`, {
      method: "PUT",
      headers: {
        accept: "*/*",
        "Content-Type": "application/json",
        Authorization: `Bearer ${PUBLIC_SWAG_TOKEN}`, // Replace with actual JWT token
      },
      body: JSON.stringify(formData),
    });

    if (response.ok) {
      console.log("Successfully updated");
      refreshEntries();
    } else {
      console.error("Failed to update:", response.status);
    }
  };

  const handleSubmit = (e) => {
    const formData = new FormData(e.target);
    for (let [name, value] of formData) {
      console.log(name, ":", value);
    }

    const passdata = {
      ...formItems,
      date: new Date().toISOString(),
      folderId: folderId > -1 ? folderId : null,
    };

    if (selectedEntry > -1) {
      // Update
      updatePassword(selectedEntry, passdata);
    } else {
      // Create
      createPassword(passdata);
    }
  };

  // Fetch folders from the API on component mount
  onMount(async () => {
    fetchFolders();
  });

  let selectedEntry = -2;

  const handleSiteSelect = (entry) => {
    selectedEntry = entry.id;
    console.log(entry);
    formItems = {
      username: entry.username,
      password: entry.decryptedPassword,
      website: entry.website,
      note: entry.note,
    };
  };

  const createNewEntry = () => {
    formItems = {
      username: "",
      password: "",
      website: "",
      note: "",
    };
    selectedEntry = -1;

    // @ts-ignore
    // focus on the first input
    document.querySelector("form input").focus();
  };

  const handlePasswordDelete = async () => {
    if (selectedEntry < 0) return;

    if (!confirm("🥺🥺🥺\nAre you sure you want to delete this password?\n👉👈")) return;

    const response = await fetch(
      `${PUBLIC_API_URL}/api/PasswordEntry/${selectedEntry}`,
      {
        method: "DELETE",
        headers: {
          accept: "*/*",
          Authorization: `Bearer ${PUBLIC_SWAG_TOKEN}`, // Replace with actual JWT token
        },
      }
    );

    if (response.ok) {
      console.log("Successfully deleted");
      refreshEntries();
    } else {
      console.error("Failed to delete:", response.status);
    }
  };

  const handleFolderDelete = async () => {
    if (folderId < 0) return;

    // if (entries.length > 0) {
    //   alert("Please delete all entries in the folder first");
    //   return;
    // }
    
    if (!confirm("Are you sure you want to delete this folder?")) return;


    const response = await fetch(`${PUBLIC_API_URL}/api/Folder/${folderId}`, {
      method: "DELETE",
      headers: {
        accept: "*/*",
        Authorization: `Bearer ${PUBLIC_SWAG_TOKEN}`, // Replace with actual JWT token
      },
    });

    if (response.ok) {
      console.log("Successfully deleted");
      fetchFolders();
    } else {
      console.error("Failed to delete:", response.status);
    }
  };

  const newFolderLogic = async () => {
    const folderName = prompt("Enter the folder name");
    if (!folderName) return;

    const response = await fetch(`${PUBLIC_API_URL}/api/Folder`, {
      method: "POST",
      headers: {
        accept: "*/*",
        "Content-Type": "application/json",
        Authorization: `Bearer ${PUBLIC_SWAG_TOKEN}`, // Replace with actual JWT token
      },
      body: JSON.stringify({ name: folderName }),
    });

    if (response.ok) {
      console.log("Successfully created");
      fetchFolders();
    } else {
      console.error("Failed to create:", response.status);
    }
  };

  let filteredPasswords = [];

  $: filteredPasswords = entries.filter((entry) =>
    entry.website.toLowerCase().includes(searchString.toLowerCase())
  );


</script>

<!-- FOLDERS -->
<header>
  <Category title="My Folders">
    {#each folders as folder}
      <li>
        <GeneralButton
          {...folder}
          active={folder.id === folderId}
          on:click={() => fetchFromId(folder.id)}
        />
        <button
          on:click={handleFolderDelete}
          class="delete"
          class:active={folder.id === folderId}
        >
          <Trash />
        </button>
      </li>
    {/each}
    <hr />
    <li>
      <GeneralButton
        active={folderId === -1}
        label="All"
        icon={WandSparkles}
        on:click={() => fetchFromId()}
      />
    </li>
  </Category>

  <ul>
    <GeneralButton
      icon={PlusIcon}
      label="New Folder"
      on:click={newFolderLogic}
    />
  </ul>
</header>
<section id="entries">
  <!-- searchbar Here -->
  <div class="search-wrapper">
    <input type="text" placeholder="Search" bind:value={searchString}
    />
    <!-- plus button -->
    <button on:click={createNewEntry} class:active={selectedEntry === -1}>
      <PlusIcon />
    </button>
  </div>

  <Category title="">
    {#if filteredPasswords.length < 1}
      <li>No passwords 🥺🥺</li>
    {/if}
    {#each filteredPasswords as entry}
      <li>
        <GeneralButton
          icon={LockKeyhole}
          label={entry.website}
          subLabel={entry.username}
          active={entry.id === selectedEntry}
          on:click={() => handleSiteSelect(entry)}
        />
      </li>
    {/each}
  </Category>
</section>
<section id="detail">
  <!-- Detail View Here -->
  <h1>CyberPass RTX5090</h1>
  <form on:submit|preventDefault={handleSubmit}>
    <hr />
    <input
      type="text"
      placeholder="Username"
      name="username"
      bind:value={formItems.username}
    />
    <input
      type="text"
      placeholder="Password"
      name="password"
      bind:value={formItems.password}
    />
    <input
      type="text"
      placeholder="Website"
      name="website"
      bind:value={formItems.website}
    />
    <hr />
    <input
      type="text"
      placeholder="Notes"
      name="note"
      bind:value={formItems.note}
    />
    {#if selectedEntry > -1}
      <button type="submit">Save</button>
      <button class="delete" on:click={handlePasswordDelete}>Delete</button>
    {:else if folderId > -2}
      <button type="submit">Create</button>
    {/if}
  </form>
</section>

<style>
  :root {
    --_clr-highlight: var(--clr-accent);
  }

  .delete {
    --_clr-highlight: #f66;
  }

  header {
    display: flex;
    flex-direction: column;
    flex: 1;
    background: var(--clr-secondary);
    padding: 1em 0.5em;
  }

  header > ul {
    margin-block-start: auto;
  }

  section {
    padding: 1em 0.5em;
    display: flex;
    background: var(--clr-primary);
    flex: 1;
    flex-direction: column;
  }

  input {
    flex: 1;
    border: none;
    color: var(--clr-text-secondary);
    background: none;
  }

  .search-wrapper {
    display: flex;
    gap: 0.5em;
    padding: 0.5em;

    & > * {
      background: var(--clr-secondary);
      border-radius: 0.3em;
      padding: 0.5em;
    }

    & button {
      display: flex;
      border: none;
      cursor: pointer;
      color: var(--clr-text-secondary);
      align-items: center;
      transition: 0.3s;
      &:hover {
        color: var(--clr-text);
        background: var(--clr-accent);
      }

      &.active {
        color: var(--clr-text);
        background: var(--clr-accent);
      }
    }
  }

  form {
    display: flex;
    flex-direction: column;
    gap: 1em;

    & input {
      padding: 0.5em;
      border-radius: 0.3em;
      border: 1px solid;

      color: var(--clr-text);
      &::placeholder {
        color: var(--clr-text-secondary);
      }
    }

    & button {
      padding: 0.5em;
      border-radius: 0.3em;
      border: none;
      cursor: pointer;
      color: var(--clr-text-secondary);
      background: var(--clr-secondary);
      transition: 0.3s;
      border: solid 2px #fff3;
      &:hover {
        color: var(--clr-text);
        border: solid 2px var(--_clr-highlight);
      }
    }

    & [name="password"] {
      filter: blur(7px);
      transition: 0.3s;
      &:focus {
        filter: blur(0px);
      }
    }
  }

  header li {
    display: flex;
  }
  header li button {
    background: var(--clr-secondary);
    border-radius: 0.3em;
    display: flex;
    border: none;
    cursor: pointer;
    color: var(--clr-text-secondary);
    align-items: center;
    transition: 0.3s;

    max-width: 0em;
    margin-left: -1em;
    opacity: 0;

    &.active {
      opacity: 1;
      margin-left: 0.3em;
      padding: 0.5em;
      max-width: 2em;
    }
    &:hover {
      color: var(--clr-text);
      background: var(--_clr-highlight);
    }
  }

  hr {
    border-color: var(--clr-text-secondary);
  }

  #entries {
    max-height: 100vh; /* Max height set to the full viewport height */
  overflow-y: auto;  /* Enables vertical scrolling when content exceeds max height */
  box-sizing: border-box; /* Ensures padding/borders are included in height calculation */
  }

  

  #detail {
    display: flex;
    gap: 1em;
  }
</style>
