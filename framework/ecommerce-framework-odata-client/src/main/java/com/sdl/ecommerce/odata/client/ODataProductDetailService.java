package com.sdl.ecommerce.odata.client;

import com.sdl.ecommerce.api.ECommerceException;
import com.sdl.ecommerce.api.ProductDetailResult;
import com.sdl.ecommerce.api.ProductDetailService;
import com.sdl.ecommerce.api.model.Product;
import com.sdl.ecommerce.api.model.impl.SimpleProductDetailResult;
import com.sdl.ecommerce.odata.model.ODataProduct;
import com.sdl.ecommerce.odata.model.ODataProductAttribute;
import com.sdl.ecommerce.odata.model.ODataProductPrice;
import com.sdl.odata.client.BasicODataClientQuery;
import com.sdl.odata.client.api.ODataClientQuery;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import javax.annotation.PostConstruct;

/**
 * OData Product Detail Service
 *
 * @author nic
 */
@Component
public class ODataProductDetailService implements ProductDetailService {

    @Autowired
    private ODataClient odataClient;

    @PostConstruct
    public void initialize() {
        this.odataClient.registerModelClass(ODataProduct.class);
        this.odataClient.registerModelClass(ODataProductPrice.class);
        this.odataClient.registerModelClass(ODataProductAttribute.class);
    }

    @Override
    public ProductDetailResult getDetail(String productId) throws ECommerceException {
        ODataClientQuery query = new BasicODataClientQuery.Builder()
                .withEntityType(ODataProduct.class)
                .withEntityKey("'" + productId + "'")
                .build();
        Product product = (Product) this.odataClient.getEntity(query);
        return new SimpleProductDetailResult(product);
    }
}
