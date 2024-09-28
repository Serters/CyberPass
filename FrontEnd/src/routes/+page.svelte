<script>
	let token = '';

	async function getUserInput() {
		let userInput = '';

		// Keep prompting until the user enters the correct password
		while (!token) {
			// Prompt the user for input
			userInput = prompt("Enter Your Master Password:");

			// If the user provides input, make the API call
			if (userInput) {
				try {
					const response = await fetch('https://localhost:7118/api/MasterPassword/login', {
						method: 'POST',
						headers: {
							'accept': '*/*',
							'Content-Type': 'application/json'
						},
						body: JSON.stringify(userInput)
					});

					if (response.ok) {
						const data = await response.json();
						token = data.token; // Save the token
						console.log('Token:', token);
						alert("Login successful!");

						// Redirect to /manager
						window.location.href = "/manager";
					} else {
						throw new Error('Invalid master password.');
					}
				} catch (error) {
					alert(error.message);
				}
			} else {
				console.log("No input provided.");
				break; // Break the loop if no input
			}
		}
	}
</script>

<section>
	<iframe src="https://www.unicorn.studio/embed/qTi0bqD6IQsN2x1VXe4a?preview=true" width="693px" height="433px" loading="lazy"></iframe>

	<div class="content">
		<h1>Never Forget a Password Again</h1>
		<!-- Button triggers the getUserInput function -->
		<button on:click={getUserInput}>Get Started</button>
	</div>
</section>


  


<style>

	
	section {
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: center;
		flex: 0.6;
		position: relative;
		overflow:hidden;

		font-family: 'Poppins', sans-serif;

		/* hide stuff */
		&::before {
			content: '';
			position: absolute;
			inset: 0;
			height: 5rem;
			top: unset;
			background: black;
			z-index: 2;
		}

		/* dim background */
		&::after {
			content: '';
			position: absolute;
			inset: 0;
			background: #0006;
			z-index: 3;
		}
	}
	

	iframe {
		width: 100%;
		height: 100vh;
		position:absolute;
		inset: 0;

		border: none;
		z-index:1;
	}

	.content {
		z-index: 4;


		height: 100vh;

		display: flex;
		justify-content: center;
		align-items: center;

		flex-direction: column;

		gap: 3em;

		font-size: .75em;


		& h1 {
			font-size: 2em;
			opacity: .7;
			font-weight: 200;

			text-shadow: 2px 4px 3px rgba(0,0,0,0.7);
			
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
		
/*  Never Forget a Password Again: Your Key to Security and Convenience
		
		no presure 25 mins left ;-;"*/
		& a {
			backdrop-filter: blur(5px);
			background: #0003;
			padding: .7em 2em;
			border-radius: 99em;
			overflow: hidden;
			font-size: 1em;
			text-transform: capitalize;

			display: flex;


			font-weight: 500;

			color: #fffb;

			border: solid 2px #fff3;

			transition: .2s;

			text-decoration: none;

			&:hover {
				background: #fff3;
				color: #fff;
			}

		}
	}
	/*  btw I got 0 idea wgats going on the preview updates once evert 5 mins lol*/

	h1 {
		width: 100%;
	}
</style>

<!--  

-->