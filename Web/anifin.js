export default function (view) {
    console.log('AniFin configuration page loaded');

    console.log("Added current token for extension pack");
    localStorage.setItem("jfs-ext", ApiClient.accessToken());

    console.log("🎯 Step 1: Starting script initialization...");

    const AniFin = {};

    AniFin.config = null;
    AniFin.queueRefreshInterval = null;
    AniFin.isPaused = false;

    // Helper: Get API URL with auth token
    AniFin.getApiUrl = function(endpoint) {
        return ApiClient.getUrl(endpoint);
    };

    // Helper: Get headers with auth token
    AniFin.getHeaders = function() {
        return {
            'X-Emby-Token': ApiClient.accessToken(),
            'Content-Type': 'application/json'
        };
    };

    console.log("🎯 Step 2: Helper functions defined");

    // Load configuration from API
    AniFin.loadConfiguration = async function() {
        console.log('🔍 Starting loadConfiguration...');
        try {
            const url = AniFin.getApiUrl('api/anifin/config');
            const headers = AniFin.getHeaders();

            console.log('📡 Fetching from URL:', url);
            console.log('🔑 Headers:', headers);

            const response = await fetch(url, { headers });

            console.log('✅ Response received:', response.status, response.statusText);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const config = await response.json();
            console.log('📦 Config received:', config);
            AniFin.config = config;

            // Wait for custom elements to be ready
            console.log('⏳ Waiting for UI elements to initialize...');
            await new Promise(resolve => setTimeout(resolve, 500));
            console.log('✅ UI should be ready now, filling fields...');

            // Fill form fields
            const downloadPathEl = view.querySelector('#downloadPath');
            const maxDownloadsEl = view.querySelector('#maxConcurrentDownloads');

            console.log('Input elements:', {
                downloadPath: downloadPathEl,
                maxDownloads: maxDownloadsEl
            });

            if (downloadPathEl) {
                downloadPathEl.value = config.DownloadPath || '';
                downloadPathEl.setAttribute('value', config.DownloadPath || '');
                downloadPathEl.dispatchEvent(new Event('change', { bubbles: true }));
                console.log('Set downloadPath to:', config.DownloadPath);
            }

            if (maxDownloadsEl) {
                maxDownloadsEl.value = config.MaxConcurrentDownloads || 2;
                maxDownloadsEl.setAttribute('value', config.MaxConcurrentDownloads || 2);
                maxDownloadsEl.dispatchEvent(new Event('change', { bubbles: true }));
                console.log('Set maxDownloads to:', config.MaxConcurrentDownloads);
            }

            // Fill language select (Settings)
            const langSelect = view.querySelector('#preferredLanguage');
            console.log('Language select element:', langSelect);

            if (langSelect) {
                langSelect.innerHTML = '';
                if (config.AvailableLanguages && Array.isArray(config.AvailableLanguages)) {
                    config.AvailableLanguages.forEach(lang => {
                        const option = document.createElement('option');
                        option.value = lang;
                        option.textContent = lang;
                        option.selected = lang === config.PreferredLanguage;
                        langSelect.appendChild(option);
                    });
                    console.log('Added', config.AvailableLanguages.length, 'language options');
                }
            }

            // Fill provider select (Settings)
            const providerSelect = view.querySelector('#preferredProvider');
            providerSelect.innerHTML = '';
            if (config.AvailableProviders && Array.isArray(config.AvailableProviders)) {
                config.AvailableProviders.forEach(provider => {
                    const option = document.createElement('option');
                    option.value = provider;
                    option.textContent = provider;
                    option.selected = provider === config.PreferredProvider;
                    providerSelect.appendChild(option);
                });
            }

            // Fill language select (Download Tab)
            const downloadLangSelect = view.querySelector('#downloadLanguage');
            downloadLangSelect.innerHTML = '<option value="">Standard verwenden</option>';
            if (config.AvailableLanguages && Array.isArray(config.AvailableLanguages)) {
                config.AvailableLanguages.forEach(lang => {
                    const option = document.createElement('option');
                    option.value = lang;
                    option.textContent = lang;
                    downloadLangSelect.appendChild(option);
                });
            }

            // Fill provider select (Download Tab)
            const downloadProviderSelect = view.querySelector('#downloadProvider');
            downloadProviderSelect.innerHTML = '<option value="">Standard verwenden</option>';
            if (config.AvailableProviders && Array.isArray(config.AvailableProviders)) {
                config.AvailableProviders.forEach(provider => {
                    const option = document.createElement('option');
                    option.value = provider;
                    option.textContent = provider;
                    downloadProviderSelect.appendChild(option);
                });
            }

            // Display read-only info
            view.querySelector('#autoProcessStatus').textContent = config.AutoProcessQueue ? 'Aktiviert' : 'Deaktiviert';
            view.querySelector('#availableLanguages').textContent = config.AvailableLanguages ? config.AvailableLanguages.join(', ') : '-';
            view.querySelector('#availableProviders').textContent = config.AvailableProviders ? config.AvailableProviders.join(', ') : '-';

        } catch (error) {
            console.error('Failed to load configuration:', error);
            Dashboard.alert('Fehler beim Laden der Konfiguration: ' + error.message);
        }
    };

    console.log("🎯 Step 3: loadConfiguration defined");

    // Save configuration
    AniFin.saveConfiguration = async function(e) {
        e.preventDefault();

        try {
            const updateData = {
                DownloadPath: view.querySelector('#downloadPath').value,
                PreferredLanguage: view.querySelector('#preferredLanguage').value,
                PreferredProvider: view.querySelector('#preferredProvider').value,
                MaxConcurrentDownloads: parseInt(view.querySelector('#maxConcurrentDownloads').value)
            };

            console.log('💾 Saving config:', updateData);

            const response = await fetch(AniFin.getApiUrl('api/anifin/config'), {
                method: 'PUT',
                headers: AniFin.getHeaders(),
                body: JSON.stringify(updateData)
            });

            console.log('📡 Save response:', response.status, response.statusText);

            if (response.ok) {
                let result = null;
                const contentType = response.headers.get('content-type');

                if (contentType && contentType.includes('application/json')) {
                    const text = await response.text();
                    if (text) {
                        try {
                            result = JSON.parse(text);
                        } catch (e) {
                            console.warn('Response is not valid JSON:', text);
                        }
                    }
                }

                console.log('✅ Config saved successfully');
                Dashboard.alert('Einstellungen gespeichert');
                await AniFin.loadConfiguration();
            } else {
                let errorMsg = 'Speichern fehlgeschlagen';
                try {
                    const text = await response.text();
                    if (text) {
                        const error = JSON.parse(text);
                        errorMsg = error.Error || error.error || errorMsg;
                    }
                } catch (e) {
                    errorMsg = `HTTP ${response.status}: ${response.statusText}`;
                }

                console.error('❌ Save failed:', errorMsg);
                Dashboard.alert('Fehler: ' + errorMsg);
            }
        } catch (error) {
            console.error('Failed to save configuration:', error);
            Dashboard.alert('Fehler beim Speichern: ' + error.message);
        }
    };

    // Detect URL type
    AniFin.detectUrlType = function(url) {
        if (!url) return null;

        if (url.includes('/staffel-') && url.includes('/episode-')) {
            return {
                type: 'episode',
                text: '🎬 Episode - Lädt eine einzelne Episode herunter'
            };
        } else if (url.includes('/staffel-')) {
            return {
                type: 'season',
                text: '📺 Staffel - Lädt eine komplette Staffel herunter'
            };
        } else if (url.includes('/anime/stream/')) {
            return {
                type: 'series',
                text: '📚 Serie - Lädt alle verfügbaren Staffeln und Episoden herunter'
            };
        }

        return null;
    };

    // Update URL type info display
    AniFin.updateUrlTypeInfo = function() {
        const url = view.querySelector('#downloadUrl').value.trim();
        const infoDiv = view.querySelector('#urlTypeInfo');
        const infoText = view.querySelector('#urlTypeText');

        const urlType = AniFin.detectUrlType(url);

        if (urlType) {
            infoText.textContent = urlType.text;
            infoDiv.style.display = 'block';
        } else {
            infoDiv.style.display = 'none';
        }
    };

    // Submit download
    AniFin.submitDownload = async function(e) {
        e.preventDefault();

        const url = view.querySelector('#downloadUrl').value.trim();
        const language = view.querySelector('#downloadLanguage').value;
        const provider = view.querySelector('#downloadProvider').value;
        const withSpecials = view.querySelector('#downloadSpecials').checked;

        if (!url) {
            Dashboard.alert('Bitte gib eine URL ein');
            return;
        }

        const urlType = AniFin.detectUrlType(url);
        if (!urlType) {
            Dashboard.alert('Ungültige URL. Bitte gib eine gültige AniWorld URL ein.');
            return;
        }

        try {
            const response = await fetch(AniFin.getApiUrl('api/anifin/queue/add'), {
                method: 'POST',
                headers: AniFin.getHeaders(),
                body: JSON.stringify({
                    Url: url,
                    Language: language || '',
                    Provider: provider || '',
                    UrlType: urlType.type,
                    WithSpecials: withSpecials
                })
            });

            if (response.ok) {
                Dashboard.alert('Download zur Queue hinzugefügt!');

                // Clear form
                view.querySelector('#downloadUrl').value = '';
                view.querySelector('#downloadLanguage').selectedIndex = 0;
                view.querySelector('#downloadProvider').selectedIndex = 0;
                view.querySelector('#downloadSpecials').checked = false;
                view.querySelector('#urlTypeInfo').style.display = 'none';

                // Switch to queue tab
                AniFin.switchTab('queue');
            } else {
                const error = await response.json();
                Dashboard.alert('Fehler: ' + (error.Error || error.error || 'Download konnte nicht hinzugefügt werden'));
            }
        } catch (error) {
            console.error('Failed to add download:', error);
            Dashboard.alert('Fehler beim Hinzufügen des Downloads: ' + error.message);
        }
    };

    // Tab switching
    AniFin.switchTab = function(tabName) {
        // Hide all tabs
        view.querySelectorAll('.tabContent').forEach(tab => {
            tab.classList.add('hide');
            tab.style.display = 'none';
        });

        // Show selected tab
        const selectedTab = view.querySelector(`#${tabName}Tab`);
        selectedTab.classList.remove('hide');
        selectedTab.style.display = 'block';

        // Update tab buttons
        view.querySelectorAll('.embyTabButton').forEach(btn => {
            btn.classList.remove('embyTabButton-active');
        });
        view.querySelector(`[data-tab="${tabName}"]`).classList.add('embyTabButton-active');

        // Stop auto-refresh when leaving queue tab
        if (tabName !== 'queue') {
            AniFin.stopQueueAutoRefresh();
        }

        // Load tab-specific data
        if (tabName === 'queue') {
            console.log('Queue tab selected');
            AniFin.loadQueueData();
            AniFin.startQueueAutoRefresh();
        } else if (tabName === 'extension') {
            console.log('Extension tab selected');
            AniFin.loadExtensionInfo();
        }
    };

    // Load extension info
    AniFin.loadExtensionInfo = async function() {
        console.log('📦 Loading extension info...');

        try {
            const response = await fetch(AniFin.getApiUrl('api/anifin/extension/info'), {
                headers: AniFin.getHeaders()
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const info = await response.json();
            console.log('Extension info:', info);

            // Update UI
            view.querySelector('#extVersion').textContent = info.Version || '-';
            view.querySelector('#extStatus').textContent = info.Available ? 'Verfügbar ✅' : 'Nicht verfügbar ❌';

            if (info.FileSizeBytes) {
                const sizeMB = (info.FileSizeBytes / 1024 / 1024).toFixed(2);
                view.querySelector('#extSize').textContent = `${sizeMB} MB`;
            } else {
                view.querySelector('#extSize').textContent = '-';
            }

            // Enable/disable download button
            const downloadBtn = view.querySelector('#downloadExtensionBtn');
            if (info.Available) {
                downloadBtn.disabled = false;
                downloadBtn.style.opacity = '1';
            } else {
                downloadBtn.disabled = true;
                downloadBtn.style.opacity = '0.5';
            }

            // Fill supported browsers
            const browsersContainer = view.querySelector('#supportedBrowsers');
            browsersContainer.innerHTML = '';

            if (info.SupportedBrowsers && info.SupportedBrowsers.length > 0) {
                info.SupportedBrowsers.forEach(browser => {
                    const badge = document.createElement('span');
                    badge.style.cssText = 'background: rgba(0,164,220,0.2); padding: 8px 15px; border-radius: 20px; font-size: 14px;';
                    badge.textContent = browser;
                    browsersContainer.appendChild(badge);
                });
            }

        } catch (error) {
            console.error('Failed to load extension info:', error);
            view.querySelector('#extStatus').textContent = 'Fehler beim Laden ❌';
        }
    };

    // Download extension
    AniFin.downloadExtension = function() {
        console.log('📥 Starting extension download...');

        const url = AniFin.getApiUrl('api/anifin/extension/download');

        // Create temporary link and trigger download
        const link = document.createElement('a');
        link.href = url;
        link.download = 'anifin-extension.zip';

        // Add auth token to URL
        const tokenParam = `api_key=${ApiClient.accessToken()}`;
        link.href = url.includes('?') ? `${url}&${tokenParam}` : `${url}?${tokenParam}`;

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        console.log('✅ Download initiated');
        Dashboard.alert('Extension-Download gestartet!');
    };

    // Queue Management
    AniFin.loadQueueData = async function() {
        console.log('📊 Loading queue data...');

        try {
            // Load statistics
            const statsResponse = await fetch(AniFin.getApiUrl('api/anifin/queue/stats'), {
                headers: AniFin.getHeaders()
            });

            if (statsResponse.ok) {
                const stats = await statsResponse.json();
                AniFin.updateQueueStats(stats);
            }

            // Load current download
            const currentResponse = await fetch(AniFin.getApiUrl('api/anifin/queue/current'), {
                headers: AniFin.getHeaders()
            });

            if (currentResponse.ok && currentResponse.status !== 204) {
                const current = await currentResponse.json();
                AniFin.displayCurrentDownload(current);
            } else {
                view.querySelector('#currentDownload').innerHTML = '<p class="fieldDescription">Kein aktiver Download</p>';
            }

            // Load history
            const historyResponse = await fetch(AniFin.getApiUrl('api/anifin/queue/history'), {
                headers: AniFin.getHeaders()
            });

            if (historyResponse.ok) {
                const history = await historyResponse.json();
                AniFin.displayQueueHistory(history);
            }

            // Load queue status for pause button
            const statusResponse = await fetch(AniFin.getApiUrl('api/anifin/queue/status'), {
                headers: AniFin.getHeaders()
            });

            if (statusResponse.ok) {
                const status = await statusResponse.json();
                AniFin.isPaused = status.IsPaused;
                AniFin.updatePauseButton();
            }

        } catch (error) {
            console.error('Failed to load queue data:', error);
        }
    };

    // Update queue statistics display
    AniFin.updateQueueStats = function(stats) {
        view.querySelector('#statTotal').textContent = stats.TotalItems || 0;
        view.querySelector('#statPending').textContent = stats.PendingItems || 0;
        view.querySelector('#statCompleted').textContent = stats.CompletedItems || 0;
        view.querySelector('#statFailed').textContent = stats.FailedItems || 0;
    };

    // Display current download with progress
    AniFin.displayCurrentDownload = function(download) {
        const container = view.querySelector('#currentDownload');

        if (!download) {
            container.innerHTML = '<p class="fieldDescription">Kein aktiver Download</p>';
            return;
        }

        const statusColor = AniFin.getStatusColor(download.Status);
        const statusText = AniFin.getStatusText(download.Status);

        container.innerHTML = `
            <div style="background: rgba(0,164,220,0.1); padding: 20px; border-radius: 8px; border-left: 4px solid ${statusColor};">
                <div style="display: flex; justify-content: space-between; align-items: start; margin-bottom: 15px;">
                    <div style="flex: 1;">
                        <h4 style="margin: 0 0 10px 0;">🎬 Aktueller Download</h4>
                        <div class="fieldDescription" style="word-break: break-all;">${download.Url}</div>
                        <div class="fieldDescription" style="margin-top: 5px;">
                            <strong>Status:</strong> ${statusText} | 
                            <strong>Type:</strong> ${download.UrlType} | 
                            <strong>Sprache:</strong> ${download.Language || 'Standard'}
                        </div>
                    </div>
                    <button is="emby-button" class="raised button-cancel" onclick="AniFin.cancelDownload('${download.Id}')" style="margin-left: 15px;">
                        <span>Abbrechen</span>
                    </button>
                </div>
                
                <!-- Progress Bar -->
                <div style="background: rgba(0,0,0,0.2); border-radius: 10px; height: 30px; overflow: hidden; position: relative;">
                    <div style="background: ${statusColor}; height: 100%; width: ${download.Progress}%; transition: width 0.3s ease;"></div>
                    <div style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); color: white; font-weight: bold; text-shadow: 0 1px 3px rgba(0,0,0,0.5);">
                        ${download.Progress}%
                    </div>
                </div>
            </div>
        `;
    };

    // Display queue history
    AniFin.displayQueueHistory = function(history) {
        const container = view.querySelector('#downloadQueue');

        if (!history || history.length === 0) {
            container.innerHTML = '<p class="fieldDescription">Keine Downloads im Verlauf</p>';
            return;
        }

        // Sort by date (newest first)
        history.sort((a, b) => new Date(b.AddedAt) - new Date(a.AddedAt));

        let html = '<div style="display: flex; flex-direction: column; gap: 10px;">';

        history.forEach(item => {
            const statusColor = AniFin.getStatusColor(item.Status);
            const statusText = AniFin.getStatusText(item.Status);
            const date = new Date(item.AddedAt).toLocaleString('de-DE');

            html += `
                <div style="background: rgba(255,255,255,0.05); padding: 15px; border-radius: 8px; border-left: 4px solid ${statusColor};">
                    <div style="display: flex; justify-content: space-between; align-items: start;">
                        <div style="flex: 1;">
                            <div class="fieldDescription" style="word-break: break-all; margin-bottom: 5px;">
                                <strong>${item.Url}</strong>
                            </div>
                            <div class="fieldDescription" style="font-size: 0.9em;">
                                <strong>Status:</strong> ${statusText} | 
                                <strong>Type:</strong> ${item.UrlType} | 
                                <strong>Hinzugefügt:</strong> ${date}
                            </div>
                            ${item.ErrorMessage ? `<div class="fieldDescription" style="color: #f44336; margin-top: 5px;">❌ ${item.ErrorMessage}</div>` : ''}
                        </div>
                        <div style="display: flex; gap: 10px; margin-left: 15px;">
                            ${item.Status === 'Failed' ? `
                                <button is="emby-button" class="raised" onclick="AniFin.retryDownload('${item.Id}')" style="min-width: 80px;">
                                    <span>Erneut</span>
                                </button>
                            ` : ''}
                            ${item.Status === 'Pending' || item.Status === 'Paused' ? `
                                <button is="emby-button" class="raised button-cancel" onclick="AniFin.cancelDownload('${item.Id}')" style="min-width: 80px;">
                                    <span>Abbrechen</span>
                                </button>
                            ` : ''}
                        </div>
                    </div>
                    ${item.Progress > 0 && item.Status === 'Downloading' ? `
                        <div style="background: rgba(0,0,0,0.2); border-radius: 5px; height: 8px; margin-top: 10px; overflow: hidden;">
                            <div style="background: ${statusColor}; height: 100%; width: ${item.Progress}%;"></div>
                        </div>
                    ` : ''}
                </div>
            `;
        });

        html += '</div>';
        container.innerHTML = html;
    };

    // Helper: Get status color
    AniFin.getStatusColor = function(status) {
        switch(status) {
            case 'Pending': return '#ffc107';
            case 'Downloading': return '#00a4dc';
            case 'Completed': return '#4caf50';
            case 'Failed': return '#f44336';
            case 'Cancelled': return '#9e9e9e';
            case 'Paused': return '#ff9800';
            default: return '#666';
        }
    };

    // Helper: Get status text
    AniFin.getStatusText = function(status) {
        switch(status) {
            case 'Pending': return '⏳ Ausstehend';
            case 'Downloading': return '⬇️ Lädt herunter';
            case 'Completed': return '✅ Abgeschlossen';
            case 'Failed': return '❌ Fehlgeschlagen';
            case 'Cancelled': return '🚫 Abgebrochen';
            case 'Paused': return '⏸️ Pausiert';
            default: return status;
        }
    };

    // Cancel download
    AniFin.cancelDownload = async function(id) {
        if (!confirm('Möchtest du diesen Download wirklich abbrechen?')) {
            return;
        }

        try {
            const response = await fetch(AniFin.getApiUrl(`api/anifin/queue/${id}/cancel`), {
                method: 'POST',
                headers: AniFin.getHeaders()
            });

            if (response.ok) {
                Dashboard.alert('Download abgebrochen');
                AniFin.loadQueueData();
            } else {
                Dashboard.alert('Fehler beim Abbrechen des Downloads');
            }
        } catch (error) {
            console.error('Failed to cancel download:', error);
            Dashboard.alert('Fehler: ' + error.message);
        }
    };

    // Retry failed download
    AniFin.retryDownload = async function(id) {
        try {
            const response = await fetch(AniFin.getApiUrl(`api/anifin/queue/${id}/retry`), {
                method: 'POST',
                headers: AniFin.getHeaders()
            });

            if (response.ok) {
                Dashboard.alert('Download wird erneut versucht');
                AniFin.loadQueueData();
            } else {
                Dashboard.alert('Fehler beim erneuten Versuch');
            }
        } catch (error) {
            console.error('Failed to retry download:', error);
            Dashboard.alert('Fehler: ' + error.message);
        }
    };

    // Pause/Resume queue
    AniFin.togglePauseQueue = async function() {
        try {
            const endpoint = AniFin.isPaused ? 'api/anifin/queue/resume' : 'api/anifin/queue/pause';
            const response = await fetch(AniFin.getApiUrl(endpoint), {
                method: 'POST',
                headers: AniFin.getHeaders()
            });

            if (response.ok) {
                AniFin.isPaused = !AniFin.isPaused;
                AniFin.updatePauseButton();
                Dashboard.alert(AniFin.isPaused ? 'Queue pausiert' : 'Queue fortgesetzt');
                AniFin.loadQueueData();
            }
        } catch (error) {
            console.error('Failed to toggle pause:', error);
            Dashboard.alert('Fehler: ' + error.message);
        }
    };

    // Update pause button text
    AniFin.updatePauseButton = function() {
        const pauseBtn = view.querySelector('#pauseQueueBtn');
        const pauseText = view.querySelector('#pauseQueueText');

        if (pauseText) {
            pauseText.textContent = AniFin.isPaused ? 'Fortsetzen' : 'Pausieren';
        }
    };

    // Clear all pending downloads
    AniFin.clearQueue = async function() {
        if (!confirm('Möchtest du wirklich alle ausstehenden Downloads löschen?')) {
            return;
        }

        try {
            const response = await fetch(AniFin.getApiUrl('api/anifin/queue/clear'), {
                method: 'POST',
                headers: AniFin.getHeaders()
            });

            if (response.ok) {
                Dashboard.alert('Queue geleert');
                AniFin.loadQueueData();
            }
        } catch (error) {
            console.error('Failed to clear queue:', error);
            Dashboard.alert('Fehler: ' + error.message);
        }
    };

    // Start auto-refresh for queue
    AniFin.startQueueAutoRefresh = function() {
        // Clear existing interval
        if (AniFin.queueRefreshInterval) {
            clearInterval(AniFin.queueRefreshInterval);
        }

        // Refresh every 2 seconds
        AniFin.queueRefreshInterval = setInterval(() => {
            AniFin.loadQueueData();
        }, 2000);

        console.log('✅ Queue auto-refresh started (every 2s)');
    };

    // Stop auto-refresh
    AniFin.stopQueueAutoRefresh = function() {
        if (AniFin.queueRefreshInterval) {
            clearInterval(AniFin.queueRefreshInterval);
            AniFin.queueRefreshInterval = null;
            console.log('🛑 Queue auto-refresh stopped');
        }
    };

    console.log("🎯 Step 4: All functions defined");

    // Global access for inline event handlers
    window.AniFin = AniFin;

    console.log("🎯 Step 5: AniFin exposed to window");

    // Initialize function
    const initialize = () => {
        console.log('🚀 Initializing AniFin...');

        // Check if required elements exist
        const configForm = view.querySelector('#aniFinConfigForm');
        const downloadForm = view.querySelector('#downloadUrlForm');

        if (!configForm || !downloadForm) {
            console.log('⏳ Forms not ready yet, retrying in 200ms...');
            setTimeout(initialize, 200);
            return;
        }

        console.log('✅ Forms found, setting up event listeners...');

        // Event Listeners
        configForm.addEventListener('submit', AniFin.saveConfiguration);
        downloadForm.addEventListener('submit', AniFin.submitDownload);

        const downloadUrlInput = view.querySelector('#downloadUrl');
        if (downloadUrlInput) {
            downloadUrlInput.addEventListener('input', () => AniFin.updateUrlTypeInfo());
        }

        // Tab buttons
        view.querySelectorAll('.embyTabButton').forEach(btn => {
            btn.addEventListener('click', function() {
                const tabName = this.getAttribute('data-tab');
                AniFin.switchTab(tabName);
            });
        });

        // Extension download button
        const downloadExtBtn = view.querySelector('#downloadExtensionBtn');
        if (downloadExtBtn) {
            downloadExtBtn.addEventListener('click', AniFin.downloadExtension);
        }

        // Queue control buttons
        const refreshQueueBtn = view.querySelector('#refreshQueueBtn');
        if (refreshQueueBtn) {
            refreshQueueBtn.addEventListener('click', () => AniFin.loadQueueData());
        }

        const pauseQueueBtn = view.querySelector('#pauseQueueBtn');
        if (pauseQueueBtn) {
            pauseQueueBtn.addEventListener('click', () => AniFin.togglePauseQueue());
        }

        const clearQueueBtn = view.querySelector('#clearQueueBtn');
        if (clearQueueBtn) {
            clearQueueBtn.addEventListener('click', () => AniFin.clearQueue());
        }

        console.log('✅ Event listeners registered');

        // Load configuration
        console.log('⚡ Loading configuration...');
        AniFin.loadConfiguration();
    };

    // Start initialization
    console.log('📋 AniFin script loaded, starting initialization...');
    setTimeout(initialize, 100);

    // Cleanup on page unload
    view.addEventListener('viewhide', function() {
        console.log('👋 AniFin page hidden, cleaning up...');
        AniFin.stopQueueAutoRefresh();
    });
}