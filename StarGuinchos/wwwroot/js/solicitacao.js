const GEOAPIFY_API_KEY = "b36368c2401c4566ab3de18228bdd7fd";

let pontoPartidaController = null;
let destinoController = null;

function debounce(fn, delay = 300) {
    let timeout;
    return function(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => fn.apply(this, args), delay);
    };
}

function somenteNumeros(valor) {
    return valor.replace(/\D/g, "");
}

function aplicarMascaraTelefone(valor) {
    const numeros = somenteNumeros(valor).slice(0, 11);

    if (numeros.length <= 2) {
        return numeros.replace(/^(\d{0,2})$/, "($1");
    }

    if (numeros.length <= 6) {
        return numeros.replace(/^(\d{2})(\d{0,4})$/, "($1) $2");
    }

    if (numeros.length <= 10) {
        return numeros.replace(/^(\d{2})(\d{4})(\d{0,4})$/, "($1) $2-$3");
    }

    return numeros.replace(/^(\d{2})(\d{5})(\d{0,4})$/, "($1) $2-$3");
}

function limparSuggestions(container) {
    container.innerHTML = "";
    container.style.display = "none";
}

function limparErro(input, erroSpan) {
    if (!input || !erroSpan) return;
    input.classList.remove("input-error");
    erroSpan.textContent = "";
}

function mostrarErro(input, erroSpan, mensagem) {
    if (!input || !erroSpan) return;
    input.classList.add("input-error");
    erroSpan.textContent = mensagem;
}

function validarFormulario() {
    const telefone = document.getElementById("telefone");
    const pontoPartida = document.getElementById("pontoPartida");
    const destino = document.getElementById("destino");

    const telefoneErro = document.getElementById("telefoneErro");
    const pontoPartidaErro = document.getElementById("pontoPartidaErro");
    const destinoErro = document.getElementById("destinoErro");

    let valido = true;

    limparErro(telefone, telefoneErro);
    limparErro(pontoPartida, pontoPartidaErro);
    limparErro(destino, destinoErro);

    const telefoneNumeros = somenteNumeros(telefone.value);

    if (!telefone.value.trim()) {
        mostrarErro(telefone, telefoneErro, "Informe o telefone.");
        valido = false;
    } else if (telefoneNumeros.length < 10) {
        mostrarErro(telefone, telefoneErro, "Informe um telefone válido.");
        valido = false;
    }

    if (!pontoPartida.value.trim()) {
        mostrarErro(pontoPartida, pontoPartidaErro, "Informe o ponto de partida.");
        valido = false;
    }

    if (!destino.value.trim()) {
        mostrarErro(destino, destinoErro, "Informe o destino.");
        valido = false;
    }

    return valido;
}

function mostrarSuggestions(container, resultados, input, latField, lngField, erroSpan) {
    container.innerHTML = "";

    if (!resultados || resultados.length === 0) {
        const semResultado = document.createElement("div");
        semResultado.className = "autocomplete-item";
        semResultado.textContent = "Nenhum endereço encontrado";
        semResultado.style.cursor = "default";
        container.appendChild(semResultado);
        container.style.display = "block";
        return;
    }

    resultados.forEach((item) => {
        const div = document.createElement("div");
        div.className = "autocomplete-item";

        const texto = item.formatted || item.address_line1 || item.address_line2 || "Endereço encontrado";
        div.textContent = texto;

        div.addEventListener("click", () => {
            input.value = texto;

            if (item.lat !== undefined && item.lat !== null) latField.value = item.lat;
            if (item.lon !== undefined && item.lon !== null) lngField.value = item.lon;

            if (erroSpan) {
                limparErro(input, erroSpan);
            }

            limparSuggestions(container);
        });

        container.appendChild(div);
    });

    container.style.display = "block";
}

async function buscarEndereco(texto, container, input, latField, lngField, tipo, erroSpan) {
    if (!texto || texto.trim().length < 3) {
        limparSuggestions(container);
        return;
    }

    container.innerHTML = '<div class="autocomplete-item" style="cursor:default;">Buscando endereço...</div>';
    container.style.display = "block";

    if (tipo === "origem" && pontoPartidaController) {
        pontoPartidaController.abort();
    }

    if (tipo === "destino" && destinoController) {
        destinoController.abort();
    }

    const controller = new AbortController();

    if (tipo === "origem") {
        pontoPartidaController = controller;
    } else {
        destinoController = controller;
    }

    try {
        const url = `https://api.geoapify.com/v1/geocode/autocomplete?text=${encodeURIComponent(texto)}&lang=pt&limit=5&filter=countrycode:br&apiKey=${GEOAPIFY_API_KEY}`;

        const response = await fetch(url, {
            method: "GET",
            signal: controller.signal
        });

        if (!response.ok) {
            throw new Error("Erro ao buscar endereços.");
        }

        const data = await response.json();

        const resultados = data.features?.map(feature => ({
            formatted: feature.properties.formatted,
            lat: feature.properties.lat,
            lon: feature.properties.lon
        })) || [];

        mostrarSuggestions(container, resultados, input, latField, lngField, erroSpan);
    } catch (error) {
        if (error.name !== "AbortError") {
            console.error("Erro no autocomplete:", error);
            container.innerHTML = '<div class="autocomplete-item" style="cursor:default;">Erro ao buscar endereços</div>';
            container.style.display = "block";
        }
    }
}

async function usarLocalizacaoAtual() {
    const input = document.getElementById("pontoPartida");
    const latField = document.getElementById("pontoPartidaLat");
    const lngField = document.getElementById("pontoPartidaLng");
    const erroSpan = document.getElementById("pontoPartidaErro");
    const botao = document.getElementById("usarLocalAtual");

    if (!navigator.geolocation) {
        alert("Seu navegador não suporta geolocalização.");
        return;
    }

    const textoOriginal = botao.textContent;
    botao.disabled = true;
    botao.textContent = "Obtendo localização...";

    navigator.geolocation.getCurrentPosition(
        async (position) => {
            const lat = position.coords.latitude;
            const lng = position.coords.longitude;

            latField.value = lat;
            lngField.value = lng;

            try {
                const url = `https://api.geoapify.com/v1/geocode/reverse?lat=${lat}&lon=${lng}&lang=pt&apiKey=${GEOAPIFY_API_KEY}`;
                const response = await fetch(url);

                if (!response.ok) {
                    throw new Error("Erro ao obter endereço da localização atual.");
                }

                const data = await response.json();
                const endereco = data.features?.[0]?.properties?.formatted;

                input.value = endereco || `${lat}, ${lng}`;
                limparErro(input, erroSpan);
            } catch (error) {
                console.error("Erro no reverse geocoding:", error);
                input.value = `${lat}, ${lng}`;
            } finally {
                botao.disabled = false;
                botao.textContent = textoOriginal;
            }
        },
        (error) => {
            console.error("Erro ao obter localização:", error);
            alert("Não foi possível obter sua localização atual.");
            botao.disabled = false;
            botao.textContent = textoOriginal;
        },
        {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 0
        }
    );
}

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("solicitacaoForm");
    const btnSolicitar = document.getElementById("btnSolicitar");

    const telefoneInput = document.getElementById("telefone");
    const pontoPartidaInput = document.getElementById("pontoPartida");
    const destinoInput = document.getElementById("destino");

    const telefoneErro = document.getElementById("telefoneErro");
    const pontoPartidaErro = document.getElementById("pontoPartidaErro");
    const destinoErro = document.getElementById("destinoErro");

    const pontoPartidaSuggestions = document.getElementById("pontoPartidaSuggestions");
    const destinoSuggestions = document.getElementById("destinoSuggestions");

    const pontoPartidaLat = document.getElementById("pontoPartidaLat");
    const pontoPartidaLng = document.getElementById("pontoPartidaLng");

    const destinoLat = document.getElementById("destinoLat");
    const destinoLng = document.getElementById("destinoLng");

    const usarLocalAtualBtn = document.getElementById("usarLocalAtual");

    const buscarOrigemComDebounce = debounce(() => {
        buscarEndereco(
            pontoPartidaInput.value,
            pontoPartidaSuggestions,
            pontoPartidaInput,
            pontoPartidaLat,
            pontoPartidaLng,
            "origem",
            pontoPartidaErro
        );
    }, 300);

    const buscarDestinoComDebounce = debounce(() => {
        buscarEndereco(
            destinoInput.value,
            destinoSuggestions,
            destinoInput,
            destinoLat,
            destinoLng,
            "destino",
            destinoErro
        );
    }, 300);

    form.addEventListener("submit", (event) => {
        const valido = validarFormulario();

        if (!valido) {
            event.preventDefault();
            return;
        }

        if (btnSolicitar) {
            btnSolicitar.disabled = true;
            btnSolicitar.textContent = "Enviando...";
        }
    });

    telefoneInput.addEventListener("input", () => {
        telefoneInput.value = aplicarMascaraTelefone(telefoneInput.value);
        limparErro(telefoneInput, telefoneErro);
    });

    pontoPartidaInput.addEventListener("input", () => {
        limparErro(pontoPartidaInput, pontoPartidaErro);
        pontoPartidaLat.value = "";
        pontoPartidaLng.value = "";
        buscarOrigemComDebounce();
    });

    destinoInput.addEventListener("input", () => {
        limparErro(destinoInput, destinoErro);
        destinoLat.value = "";
        destinoLng.value = "";
        buscarDestinoComDebounce();
    });

    usarLocalAtualBtn.addEventListener("click", usarLocalizacaoAtual);

    document.addEventListener("click", (event) => {
        if (!pontoPartidaSuggestions.contains(event.target) && event.target !== pontoPartidaInput) {
            limparSuggestions(pontoPartidaSuggestions);
        }

        if (!destinoSuggestions.contains(event.target) && event.target !== destinoInput) {
            limparSuggestions(destinoSuggestions);
        }
    });
});