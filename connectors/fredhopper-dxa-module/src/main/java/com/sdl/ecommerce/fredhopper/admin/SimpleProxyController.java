package com.sdl.ecommerce.fredhopper.admin;

import org.apache.commons.httpclient.*;
import org.apache.commons.httpclient.auth.AuthScope;
import org.apache.commons.httpclient.methods.GetMethod;
import org.apache.commons.io.IOUtils;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;

import javax.annotation.PostConstruct;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import java.io.IOException;
import java.net.URLEncoder;
import java.util.Enumeration;

/**
 * Simple Proxy Controller
 *
 * @author nic
 */
@Controller
@RequestMapping({ "/heatmap", "/preview" })
public class SimpleProxyController {

    @Value("${fredhopper.adminserver.url}")
    private String fredhopperBaseUrl = "http://localhost:8180";
    @Value("${fredhopper.access.username}")
    private String accessUsername = null;
    @Value("${fredhopper.access.password}")
    private String accessPassword = null;

    private HttpClient client;


    @PostConstruct
    public void initialize() throws IOException {

        MultiThreadedHttpConnectionManager connectionManager =
                new MultiThreadedHttpConnectionManager();
        this.client = new HttpClient(connectionManager);
        if ( this.accessUsername != null && !this.accessUsername.isEmpty() ) {
            Credentials credentials = new UsernamePasswordCredentials(this.accessUsername, this.accessPassword);
            client.getState().setCredentials(AuthScope.ANY, credentials);
        }
    }


    @RequestMapping(method = RequestMethod.GET, value = "/**", produces = {MediaType.ALL_VALUE})
    public void proxyAssets(HttpServletRequest request, HttpServletResponse response) throws IOException {

        final String requestPath = request.getRequestURI();
        final boolean isAjax = request.getHeader("X-Requested-With") != null;
        String fredhopperUrl = this.fredhopperBaseUrl + requestPath.replace(".fhjsp", ".jsp") + this.getQueryString(request);
        GetMethod method = new GetMethod(fredhopperUrl);
        if ( isAjax ) {
            Enumeration<String> headerNames = request.getHeaderNames();
            while (headerNames.hasMoreElements()) {
                String headerName = headerNames.nextElement();
                if (headerName.startsWith("wicket") || headerName.startsWith("x-") /*|| headerName.startsWith("accept")*/ || headerName.startsWith("user-agent")) {
                    method.setRequestHeader(headerName, request.getHeader(headerName));
                }
            }
        }
        int statusCode = client.executeMethod(method);

        if ( statusCode == HttpStatus.SC_OK ) {
            // TODO: Fix encoding - because it impacts some of the special characters
            //
            //Header contentType = method.getResponseHeader("Content-Type");
            //response.setContentType(contentType.getValue());
            IOUtils.copy(method.getResponseBodyAsStream(), response.getOutputStream());
            response.flushBuffer();
        }
        else {
            response.sendError(statusCode);
        }
        method.releaseConnection();
    }

    protected String getQueryString(HttpServletRequest request) {
        if (request.getQueryString() == null ) { return ""; }
        StringBuilder queryString = new StringBuilder();
        queryString.append("?");
        Enumeration<String> parameterNames = request.getParameterNames();
        while ( parameterNames.hasMoreElements() ) {
            String parameterName = parameterNames.nextElement();
            queryString.append(parameterName);
            queryString.append("=");
            queryString.append(URLEncoder.encode(request.getParameter(parameterName)));
            if ( parameterNames.hasMoreElements() ) {
                queryString.append("&");
            }
        }
        return queryString.toString();
    }

}

