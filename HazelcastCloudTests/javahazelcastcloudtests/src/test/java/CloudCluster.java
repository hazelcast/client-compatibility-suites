public class CloudCluster {
    public String getId() {
        return id;
    }

    public void setId(String id) {
        this.id = id;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getReleaseName() {
        return releaseName;
    }

    public void setReleaseName(String releaseName) {
        this.releaseName = releaseName;
    }

    public String getHazelcastVersion() {
        return hazelcastVersion;
    }

    public void setHazelcastVersion(String hazelcastVersion) {
        this.hazelcastVersion = hazelcastVersion;
    }

    public boolean isTlsEnabled() {
        return isTlsEnabled;
    }

    public void setTlsEnabled(boolean tlsEnabled) {
        isTlsEnabled = tlsEnabled;
    }

    public String getState() {
        return state;
    }

    public void setState(String state) {
        this.state = state;
    }

    public String getToken() {
        return token;
    }

    public void setToken(String token) {
        this.token = token;
    }

    public String getCertificatePath() {
        return certificatePath;
    }

    public void setCertificatePath(String certificatePath) {
        this.certificatePath = certificatePath;
    }

    public String getTlsPassword() {
        return tlsPassword;
    }

    public void setTlsPassword(String tlsPassword) {
        this.tlsPassword = tlsPassword;
    }

    public CloudCluster(String id, String name, String releaseName, String hazelcastVersion, boolean isTlsEnabled, String state, String token, String certificatePath, String tlsPassword) {
        this.id = id;
        this.name = name;
        this.releaseName = releaseName;
        this.hazelcastVersion = hazelcastVersion;
        this.isTlsEnabled = isTlsEnabled;
        this.state = state;
        this.token = token;
        this.certificatePath = certificatePath;
        this.tlsPassword = tlsPassword;
    }

    public String id;
    public String name;
    public String releaseName;
    public String hazelcastVersion;
    public boolean isTlsEnabled;
    public String state;
    public String token;
    public String certificatePath;
    public String tlsPassword;
}